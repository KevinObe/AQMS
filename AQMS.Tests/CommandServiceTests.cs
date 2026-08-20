using AQMS.Web.Data;
using AQMS.Web.Models;
using AQMS.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace AQMS.Tests;

public class CommandServiceTests
{
    // Jeder Test bekommt eine eigene InMemory-DB (Guid als Name).
    // Ohne das würden sich die Tests gegenseitig die Daten verändern -
    // xUnit führt Testklassen parallel aus.
    private static AqmsDbContext NeuerContext()
    {
        var options = new DbContextOptionsBuilder<AqmsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var db = new AqmsDbContext(options);

        // Legt das Modell an und spielt die HasData-Seed-Daten ein:
        // Device 1 = raspberry-pi (Sensor), Device 2-6 = Shellys (SmartPlug)
        db.Database.EnsureCreated();

        return db;
    }

    [Fact]
    public async Task CreateCommandAsync_GueltigesGeraet_LegtPendingBefehlAn()
    {
        using var db = NeuerContext();
        var service = new CommandService(db);

        var ergebnis = await service.CreateCommandAsync(2, DeviceState.On, "user-abc");

        Assert.Equal(CreateCommandResult.Success, ergebnis);

        var befehl = await db.DeviceCommands.SingleAsync();
        Assert.Equal(2, befehl.DeviceId);
        Assert.Equal(DeviceState.On, befehl.Action);
        Assert.Equal(CommandStatus.Pending, befehl.Status);
        // Beweist, dass die UserId serverseitig durchgereicht wird
        Assert.Equal("user-abc", befehl.RequestedByUserId);
    }

    [Fact]
    public async Task CreateCommandAsync_UnbekanntesGeraet_LiefertDeviceNotFound()
    {
        using var db = NeuerContext();
        var service = new CommandService(db);

        var ergebnis = await service.CreateCommandAsync(999, DeviceState.On, null);

        Assert.Equal(CreateCommandResult.DeviceNotFound, ergebnis);
        Assert.Empty(db.DeviceCommands);
    }

    [Fact]
    public async Task CreateCommandAsync_DeaktiviertesGeraet_LiefertDeviceDisabled()
    {
        using var db = NeuerContext();

        // Gerät gezielt deaktivieren
        var device = await db.Devices.SingleAsync(d => d.Id == 3);
        device.IsEnabled = false;
        await db.SaveChangesAsync();

        var service = new CommandService(db);

        var ergebnis = await service.CreateCommandAsync(3, DeviceState.Off, null);

        Assert.Equal(CreateCommandResult.DeviceDisabled, ergebnis);
        // Kein Befehl angelegt - er würde sonst ewig Pending bleiben,
        // weil GetPendingCommands deaktivierte Geräte herausfiltert.
        Assert.Empty(db.DeviceCommands);
    }

    [Fact]
    public async Task CreateCommandAsync_BereitsOffenerBefehl_LiefertAlreadyPending()
    {
        using var db = NeuerContext();
        var service = new CommandService(db);

        // Erster Befehl geht durch
        await service.CreateCommandAsync(2, DeviceState.On, null);

        // Zweiter Klick auf dasselbe Gerät wird abgewiesen (Idempotenz-Riegel
        // gegen Relais-Flattern, wenn der Pi offline ist)
        var ergebnis = await service.CreateCommandAsync(2, DeviceState.Off, null);

        Assert.Equal(CreateCommandResult.AlreadyPending, ergebnis);
        Assert.Equal(1, await db.DeviceCommands.CountAsync());
    }

    [Fact]
    public async Task GetPendingCommands_LiefertNurSmartPlugsMitIp()
    {
        using var db = NeuerContext();

        // Ein Befehl für den Pi (Sensor) - darf nicht im Poll auftauchen
        db.DeviceCommands.Add(new DeviceCommand
        {
            DeviceId = 1,
            Action = DeviceState.On,
            Status = CommandStatus.Pending,
            CreatedAt = DateTime.UtcNow
        });

        // Ein Befehl für einen Shelly - muss auftauchen
        db.DeviceCommands.Add(new DeviceCommand
        {
            DeviceId = 2,
            Action = DeviceState.On,
            Status = CommandStatus.Pending,
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var service = new CommandService(db);
        var pending = await service.GetPendingCommands();

        var einziger = Assert.Single(pending);
        Assert.Equal("shelly-filter", einziger.DeviceIdentifier);
        Assert.Equal("10.0.0.227", einziger.IPAddress);
        Assert.Equal("On", einziger.Action);
    }

    [Fact]
    public async Task ProcessCommandResult_Erfolg_SetztExecutedUndSchreibtStateChange()
    {
        using var db = NeuerContext();
        var service = new CommandService(db);

        await service.CreateCommandAsync(2, DeviceState.On, "user-abc");
        var befehlId = (await db.DeviceCommands.SingleAsync()).Id;

        var ergebnis = await service.ProcessCommandResult(new Web.Dtos.CommandResultDto
        {
            CommandId = befehlId,
            Success = true,
            ResultMessage = "Test ok"
        });

        Assert.Equal(CommandResult.Success, ergebnis);

        var befehl = await db.DeviceCommands.SingleAsync();
        Assert.Equal(CommandStatus.Executed, befehl.Status);
        Assert.NotNull(befehl.ExecutedAt);

        // Der StateChange ist der Audit-Trail: wer hat wann was geschaltet
        var stateChange = await db.StateChanges.SingleAsync();
        Assert.Equal(DeviceState.On, stateChange.State);
        Assert.Equal("user-abc", stateChange.ChangedByUserId);

        // Und der Gerätezustand wurde nachgezogen
        var device = await db.Devices.SingleAsync(d => d.Id == 2);
        Assert.Equal(DeviceState.On, device.CurrentState);
    }

    [Fact]
    public async Task ProcessCommandResult_ZweiteMeldung_LiefertAlreadyProcessed()
    {
        using var db = NeuerContext();
        var service = new CommandService(db);

        await service.CreateCommandAsync(2, DeviceState.On, null);
        var befehlId = (await db.DeviceCommands.SingleAsync()).Id;

        var dto = new Web.Dtos.CommandResultDto { CommandId = befehlId, Success = true };

        await service.ProcessCommandResult(dto);

        // Der Worker meldet at-least-once - eine doppelte Meldung muss abprallen,
        // sonst entstünde ein zweiter StateChange.
        var zweitesErgebnis = await service.ProcessCommandResult(dto);

        Assert.Equal(CommandResult.AlreadyProcessed, zweitesErgebnis);
        Assert.Equal(1, await db.StateChanges.CountAsync());
    }

    [Fact]
    public async Task ProcessCommandResult_UnbekannteId_LiefertCommandNotFound()
    {
        using var db = NeuerContext();
        var service = new CommandService(db);

        var ergebnis = await service.ProcessCommandResult(new Web.Dtos.CommandResultDto
        {
            CommandId = 999,
            Success = true
        });

        Assert.Equal(CommandResult.CommandNotFound, ergebnis);
    }
}