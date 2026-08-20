using AQMS.Web.Data;
using AQMS.Web.Dtos;
using AQMS.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AQMS.Web.Services;

//Enum als rückgabewert des service
//das enum wird nach rückgabe vom Controller weiter verarbeitet deshalb kein Task<ActionResult>
public enum CommandResult
{
    Success,
    CommandNotFound,
    AlreadyProcessed
}

//Eigenes Enum für die Befehls-erstellung - bewusst nicht CommandResult wiederverwendet;
//Erstellung und Ergebnis-Verarbeitung haben komplett andere Fehlerfälle (Gerät unbekannt/deaktiviert
//vs. Befehl unbekannt/schon verarbeitet). Ein gemeinsames Enum hätte in beiden Aufrufern
//tote Zweige erzeugt, die man nie treffen kann;
public enum CreateCommandResult
{
    Success,
    DeviceNotFound,
    DeviceDisabled,
    AlreadyPending
}


//Service
//mehrstufige Dependency Injection; 
//controller braucht CommandService - commandservice braucht aqmsdbContext; - DI wird verkettet;
// Der Controller ruft diese Klasse auf, diese Klasse spricht mit dem DbContext.
// Der Service kennt bewusst keine HTTP-Typen (kein ActionResult) -> bleibt ohne ASP.NET testbar.
public class CommandService
{
    //DbContext wird per Konstruktor-Injektion vom DI-Container bereitgestellt; im Hintergrund registriert in Program.cs via AddDbContext
    //muss auf db zugreifen für post requests und get
    //braucht db context
    // readonly, wird genau einmal zugewiesen und bleibt so, verhindert überschreiben von _db während DB-Verbindung mitten in einer methode;
    private readonly AqmsDbContext _db;

    //constructor braucht db context, wird mittels parameter eingefügt; 

    public CommandService(AqmsDbContext db)
    {
        _db = db;
    }

    //Pi-Worker holt alle Befehle in einem Poll
    // Rückgabetyp nicht nullable - keine unbekannten geräte möglich
    public async Task<List<CommandPendingDto>> GetPendingCommands()
    {
        // erst laden (SQL inkl. Join auf Device), dann mappen (C#):
        //device in db suchen; 
        var pendingCommands = await _db.DeviceCommands
            .Include(c => c.Device)
            .Where(c => c.Status == CommandStatus.Pending
                     && c.Device.DeviceType.Name == "SmartPlug"   // nicht der Pi
                     && c.Device.IsEnabled                        // deaktivierte Geräte nicht schalten
                     && c.Device.IPAddress != null)               // ohne IP nicht ausführbar - wird in request mitgesendet
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        //server seitige operation - hier wird in sql übersetzt und gearbeitet
        //alle commands mit status pending aus db auslesen und zurückgeben; 
        //zu liste und diese wird auf DTO CommandPendingDto geampped; 
        //ab hier c# client seitig, muss nicht mehr in sql übersetzt werden;
        return pendingCommands.Select(c => new CommandPendingDto
        {
            CommandId = c.Id,
            Action = c.Action.ToString(),
            CreatedAt = c.CreatedAt,
            DeviceIdentifier = c.Device.DeviceIdentifier,
            IPAddress = c.Device.IPAddress,
        }).ToList();
    }

    //Methode zum antworten des verarbeitungszustandes mittels CommandResultdto;
    //change tracking: kein klassisches sql schreiben mit update befehlen sondern durch den reingeladenen dbcontext wird dieser direkt bearbeitet und danach in die db gespeichert;
    public async Task<CommandResult> ProcessCommandResult(CommandResultDto dto)
    {
        //variable mit aktueller Zeit um beim setzen keine abweichungen zu haben;
        var timestampNow = DateTime.UtcNow;

        //deviceCommand aus db auslesen; 
        //dazu prüfen ob das übergebene dto entsprechend existiert in der db
        //mittels include wird zusätzlich ein join auf devices gemacht um dieses zusätzlich zu prüfen / matchen;
        var command = await _db.DeviceCommands.Include(c => c.Device).FirstOrDefaultAsync(c => c.Id == dto.CommandId);

        //ergibt die db operation null - existiert kein entsprechender Befehl;
        if (command is null) return CommandResult.CommandNotFound;

        //prüfen ob der befehlsstatus wirklich pending ist, sonst kann es bei netzwerkproblemen zu zweitem stateChange kommen;
        if (command.Status != Models.CommandStatus.Pending) return CommandResult.AlreadyProcessed;

        //timestamp setzen vor weiterer verarbeitung;
        command.ExecutedAt = timestampNow;

        //Ergebnis Message zuweisen;
        command.ResultMessage = dto.ResultMessage;

        if (dto.Success)
        {
            //befehlsstatus updaten wenn ausgeführt wurde
            command.Status = CommandStatus.Executed;

            //neues statechange update erstellen mittels objekt initialisierung
            var stateChange = new StateChange
            {
                DeviceId = command.DeviceId,
                Timestamp = timestampNow,
                State = command.Action,
                ChangedByUserId = command.RequestedByUserId
            };

            //object als added markieren ums beim speichern am ende in die db zu schreiben;
            _db.StateChanges.Add(stateChange);

            //neue werte setzen für den neuen state und die neue last seen zeit 
            command.Device.CurrentState = command.Action;
            command.Device.LastSeen = timestampNow;

        }
        else
        {
            //wenn was nicht funktioniert, nicht updaten und status des befehls als failed markieren;
            command.Status = CommandStatus.Failed;
        }

        //nun die datenbank updaten;
        await _db.SaveChangesAsync();

        //methode beenden und status enum zurück geben;
        return CommandResult.Success;
    }

    //Methode zum anlegen eines Befehls (Gegenstück zu GetPendingCommands);
    //Auslöser ist der Dashboard-Toggle (MVC-Route, Cookie-Auth) - nicht der Worker;
    //der Worker holt Befehle nur ab, er erzeugt keine;
    //
    //userId kommt serverseitig aus dem eingeloggten Benutzer (User.GetUserId im Controller)
    //und wird bewusst nicht aus dem Request-Body gebunden - sonst könnte ein Client eine
    //fremde UserId mitschicken und Befehle unter falschem Namen protokollieren;
    public async Task<CreateCommandResult> CreateCommandAsync(int deviceId, DeviceState action, string? userId)
    {
        //device aus db laden, prüfen ob es das gerät überhaupt gibt;
        var device = await _db.Devices.FirstOrDefaultAsync(d => d.Id == deviceId);

        //null = kein passendes device in der db;
        if (device is null) return CreateCommandResult.DeviceNotFound;

        //deaktivierte geräte werden von GetPendingCommands ohnehin herausgefiltert;
        //ein befehl dafür würde also ewig auf Pending stehen bleiben - gar nicht erst anlegen;
        if (!device.IsEnabled) return CreateCommandResult.DeviceDisabled;

        //Idempotenz-Riegel: ist der Pi offline, würde jeder klick einen weiteren Pending-Befehl
        //erzeugen. Beim wiederanlauf würde der worker sie alle nacheinander abarbeiten
        //-> das relais würde mehrfach hin und her schalten (relais-flattern).
        //ein offener befehl pro gerät genügt;
        //
        //vergleich direkt gegen den enum-wert, nicht c.Status.ToString() == "Pending" -
        //ToString() ist nicht nach SQL übersetzbar, der enum-vergleich
        //dagegen schon, weil HasConversion<string>() im DbContext konfiguriert ist;
        var hasPending = await _db.DeviceCommands
            .AnyAsync(c => c.DeviceId == deviceId && c.Status == CommandStatus.Pending);

        if (hasPending) return CreateCommandResult.AlreadyPending;

        //neuen befehl per objekt-initialisierung bauen;
        //CreatedAt wird hier explizit gesetzt obwohl die DB einen GETUTCDATE()-Default hat -
        //so steht der wert sofort im change tracker zur verfügung und ist nicht erst nach
        //einem reload aus der db sichtbar;
        var command = new DeviceCommand
        {
            DeviceId = deviceId,
            Action = action,
            Status = CommandStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            RequestedByUserId = userId
        };

        //ohne Add() wird die neue entity still nicht persistiert - kein fehler, nur kein insert;
        _db.DeviceCommands.Add(command);

        await _db.SaveChangesAsync();

        return CreateCommandResult.Success;
    }
}