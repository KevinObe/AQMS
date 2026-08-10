using AQMS.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AQMS.Web.Data;

// public um von draussen zugreifen zu können, ohne public = default private
//bei der vererbung IdenitityUser miteinbeziehen, liefert einige wichtige properties für authentifizierung etc...
public class AqmsDbContext(DbContextOptions<AqmsDbContext> options) : IdentityDbContext<IdentityUser>(options)
{
    //DbSet<Klassenname singular> Tabellenname plural { getter und setter }
    public DbSet<DeviceType> DeviceTypes { get; set; } = null!;
    public DbSet<MeasurementType> MeasurementTypes { get; set; } = null!;
    public DbSet<Device> Devices {  get; set; } = null!;
    public DbSet<Measurement> Measurements { get; set; } = null!;
    public DbSet<DeviceCommand> DeviceCommands { get; set; } = null!;

    public DbSet<StateChange> StateChanges { get; set; } = null!;

    // ---------------------------------------------------------------------------
    // FLUENT-API REFERENZ - kommentare dienen zum lernen hier; 
    //
    //OnModelCreating Methode wird gebraucht um vor dem Bau der Tabellen noch settings oder configs zu übergeben.
    //zuerst werden die tabellen mit dbset benannt, mit den klassen die spalten definiert und mit OnModelCreating werden weitere konfigurationen, dann wird das Schema aufgebaut;
    //diese fluent-API bietet eine methode spalten zu spezifizieren statt der data annotation wie [MaxLength(50)] direkt an der property und hat mehr funktionen als die annotation;
    //Beispiele:
    // String-Länge: ohne Konfig wäre alles nvarchar(max), zu groß
    //builder.Entity<Device>()
    //.Property(d => d.Name)
    //.HasMaxLength(50);
    //
    // Decimal-Präzision für Geldbeträge
    //builder.Entity<Order>()
    //.Property(o => o.Amount)
    //.HasPrecision(18, 2);
    //
    // Pflichtfeld erzwingen (wenn Konvention das nicht erkennt)
    //builder.Entity<Device>()
    //.Property(d => d.DeviceIdentifier)
    //.IsRequired();
    //
    // OnDelete-Verhalten ändern (Default ist Cascade!)
    //builder.Entity<Measurement>()
    //.HasOne(m => m.Device)
    //.WithMany(d => d.Measurements)
    //.HasForeignKey(m => m.DeviceId)
    //.OnDelete(DeleteBehavior.Restrict);
    //
    // UNIQUE-Constraint
    //builder.Entity<Device>()
    //.HasIndex(d => d.DeviceIdentifier)
    //.IsUnique();
    //
    // Composite Index
    //builder.Entity<Measurement>()
    //.HasIndex(m => new { m.DeviceId, m.Timestamp });
    //
    // DB-seitiger Default
    // builder.Entity<Measurement>()
    // .Property(m => m.Timestamp)
    // .HasDefaultValueSql("GETUTCDATE()");
    //
    // Seed-Daten
    //builder.Entity<DeviceType>()
    //.HasData(
    //    new DeviceType { Id = 1, Name = "Sensor" },
    //    new DeviceType { Id = 2, Name = "SmartPlug" }
    //);
    // 
    // String-Länge:    builder.Entity<X>().Property(...).HasMaxLength(N)
    // Pflichtfeld:     ... .IsRequired()
    // Composite Index: builder.Entity<X>().HasIndex(x => new { x.A, x.B })
    // UNIQUE:          builder.Entity<X>().HasIndex(...).IsUnique()
    // OnDelete:        ... .OnDelete(DeleteBehavior.Restrict)
    // Default-Wert:    ... .HasDefaultValueSql("GETUTCDATE()")
    // Seed-Daten:      builder.Entity<X>().HasData(new X {...})
    // ---------------------------------------------------------------------------
    protected override void OnModelCreating(ModelBuilder builder)
    {
        //diese zeile configuriert die identity tabellen, ohne diese führts zu viele fehlern
        base.OnModelCreating(builder);

        builder.Entity<MeasurementType>(entity =>
        {
            entity.Property(m => m.Name)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(m => m.Unit)
                  .IsRequired()
                  .HasMaxLength(10);

            entity.HasData(
                    new MeasurementType { Id = 1, Name = "Temperature", Unit = "°C" },
                    new MeasurementType { Id = 2, Name = "Power", Unit = "W" }
                );
        });

        builder.Entity<DeviceType>(entity =>
        {
            entity.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(50);

            entity.HasData(
                    new DeviceType { Id = 1, Name = "Sensor" },
                    new DeviceType { Id = 2, Name = "SmartPlug" }
                );
        });

        builder.Entity<Device>(entity =>
        {
            entity.Property(d => d.CurrentState)
            .HasConversion<string>()
            .HasMaxLength(20);

            entity.Property(d => d.DeviceIdentifier)
            .IsRequired()
            .HasMaxLength(100);

            entity.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(50);

            //ipv6 länge bei bedarf 45 zeichen
            entity.Property(d => d.IPAddress)
            .HasMaxLength(45);

            //unique index erlaubt null und ist eindeutig
            //must have auf db ebene damit vermieden wird, dass 2 devices den selben identifier kriegen
            entity.HasIndex(d => d.DeviceIdentifier)
            .IsUnique();

            //Was passiert beim Parent-Delete
            //Cascade Children werden mitgelöscht(Default in EF Core!) - würde alle messdaten mit device mitlöschen
            //Restrict Parent kann nicht gelöscht werden, solange Children existieren - device erst löschen wenn zuerst daten gelöscht wurden
            //SetNull Children - FK wird auf null gesetzt(nur bei nullable FKs möglich)

            //Ein Device hat einen DeviceType, ein DeviceType hat viele Devices, FK heißt DeviceTypeId, beim Löschen blockieren.
            entity.HasOne(d => d.DeviceType)
            .WithMany(dt => dt.Devices)
            .HasForeignKey(d => d.DeviceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

            entity.HasData(
                    new Device
                    {
                        Id = 1,
                        DeviceIdentifier = "raspberry-pi",
                        Name = "AQMS - RaspberryPi",
                        DeviceTypeId = 1,
                        IPAddress = "10.0.0.222",
                        IsEnabled = true,
                    },

                    new Device
                    {
                        Id = 2,
                        DeviceIdentifier = "shelly-filter",
                        Name = "Filter",
                        DeviceTypeId = 2,
                        IPAddress = "10.0.0.227",
                        IsEnabled = true,
                    },

                    new Device
                    {
                        Id = 3,
                        DeviceIdentifier = "shelly-light",
                        Name = "Light",
                        DeviceTypeId = 2,
                        IPAddress = "10.0.0.226",
                        IsEnabled = true,
                    },

                    new Device
                    {
                        Id = 4,
                        DeviceIdentifier = "shelly-co2",
                        Name = "CO2",
                        DeviceTypeId = 2,
                        IPAddress = "10.0.0.225",
                        IsEnabled = true,
                    },

                    new Device
                    {
                        Id = 5,
                        DeviceIdentifier = "shelly-heater",
                        Name = "Heater",
                        DeviceTypeId = 2,
                        IPAddress = "10.0.0.224",
                        IsEnabled = true,
                    },

                    new Device
                    {
                        Id = 6,
                        DeviceIdentifier = "shelly-skimmer",
                        Name = "Skimmer",
                        DeviceTypeId = 2,
                        IPAddress = "10.0.0.223",
                        IsEnabled = true,
                    }
                );
        });

        builder.Entity<DeviceCommand>(entity =>
        {
            //mehr zeichen für eventuell fehlermeldungen etc
            entity.Property(c => c.ResultMessage)
            .HasMaxLength(200);

            //Identity Framework default länge für UserIDs -> 450
            entity.Property(c => c.RequestedByUserId)
            .HasMaxLength(450);

            entity.Property(c => c.Action)
            .HasConversion<string>()
            .HasMaxLength(10);

            entity.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(CommandStatus.Pending);

            entity.HasOne<IdentityUser>()
            .WithMany()
            .HasForeignKey(c => c.RequestedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

            // Pflichtbeziehung zum Device — beim Löschen blockieren
            entity.HasOne(c => c.Device)
            .WithMany(d => d.Commands)
            .HasForeignKey(c => c.DeviceId)
            .OnDelete(DeleteBehavior.Restrict);

            // Filtered Index: nur pending Befehle indizieren — fuer das Pi-Polling
            entity.HasIndex(c => c.Status)
            .HasFilter("[Status] = 'Pending'");

            //default wert wird bei insert gesetzt für genaue erfassung der zeit
            entity.Property(c => c.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
        });

        builder.Entity<Measurement>(entity =>
        {
            // Pflichtbeziehung zum Device — beim Löschen blockieren
            entity.HasOne(m => m.Device)
            .WithMany(d => d.Measurements)
            .HasForeignKey(m => m.DeviceId)
            .OnDelete(DeleteBehavior.Restrict);

            // Pflichtbeziehung zum MeasurementType — beim Löschen blockieren
            entity.HasOne(m => m.MeasurementType)
            .WithMany(mt => mt.Measurements)
            .HasForeignKey(m => m.MeasurementTypeId)
            .OnDelete(DeleteBehavior.Restrict);

            // Composite Index: schneller Lookup "alle Werte eines Geraets in Zeitraum X"
            entity.HasIndex(m => new { m.DeviceId, m.Timestamp });

            // Globaler Index auf Timestamp (z.B. Cleanup-Jobs, Gesamt-Queries)
            entity.HasIndex(m => m.Timestamp);

            //setzen einer default time stamp beim insert in die db falls der worker nix liefert
            entity.Property(m => m.Timestamp)
            .HasDefaultValueSql("GETUTCDATE()");
        });

        builder.Entity<StateChange>(entity =>
        {
            entity.Property(s => s.ChangedByUserId)
            .HasMaxLength(450);

            //string convertion des enum um nicht als int zu speichern (konsistenz)
            entity.Property(s => s.State)
            .HasConversion<string>()
            .HasMaxLength(10);

            //Pflichtbeziehung zum Device
            entity.HasOne(s => s.Device)
            .WithMany(d => d.StateChanges)
            .HasForeignKey(s => s.DeviceId)
            .OnDelete(DeleteBehavior.Restrict);

            //optionale Beziehung zum User
            entity.HasOne<IdentityUser>()
            .WithMany()
            .HasForeignKey(s => s.ChangedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

            // Composite Index für "alle State-Wechsel eines Devices in Zeitraum X"
            entity.HasIndex(s => new { s.DeviceId, s.Timestamp });

            //zeitstempel als default setzen wenn in db gespeichert wird
            entity.Property(s => s.Timestamp)
            .HasDefaultValueSql("GETUTCDATE()");
        });
    }
}


