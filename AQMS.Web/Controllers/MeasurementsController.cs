using AQMS.Web.Data;
using AQMS.Web.Dtos;
using AQMS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AQMS.Web.Controllers;

//apicontroller - schaltet ASP.NETs API-Verhalten ein. Konkret: automatische Modell-Validierung (bei ungültigem Request-Body → automatisch 400 Bad Request
//behandle Klasse als API, nicht als MVC-Notbehelf
//rout definiert den basispfad auf welche der controller angewandt wird
[ApiController]
[Route("api/measurements")]
public class MeasurementsController : ControllerBase
{

    //Konstruktor-Injektion ist der Default- und Standard-Stil für Controller in ASP.NET Core.
    //Das Konzept: Der DI-Container sieht beim Erzeugen einer Controller-Instanz nach, welche Parameter dein Konstruktor erwartet, fügt sie automatisch ein, und übergibt sie.
    //man muss nicht explizit fragen — die Verdrahtung passiert über die Parameter-Liste.

    //DbContext wird per Konstruktor-Injektion vom DI-Container bereitgestellt; im Hintergrund registriert in Program.cs via AddDbContext
    //muss auf db zugreifen für post requests und get
    //braucht db context
    private readonly AqmsDbContext _db;

    //Konstruktor
    public MeasurementsController(AqmsDbContext db)
    {
        _db = db;
    }

    //GET
    //Hier wird die Methode zur antwort auf GET requests definiert
    //return value ist ein Task (async)
    [HttpGet]
    public async Task<ActionResult> GetByDevice(string deviceIdentifier, string typeName, int limit = 100)
    {
        //device lookup, check ob das gesendete device existiert per identifier
        var device = await _db.Devices.FirstOrDefaultAsync(d => d.DeviceIdentifier == deviceIdentifier);

        //400 wenn null
        if (device is null) return BadRequest($"Unknown Device-Identifier: {deviceIdentifier}");

        //check ob der request typeName existiert in den measurementTypes
        var measurementType = await _db.MeasurementTypes.FirstOrDefaultAsync(t => t.Name == typeName);

        //400 wenn null
        if (measurementType is null) return BadRequest($"Unknown Measurement-Type: {typeName}");

        //messwerte auslesen und vorbereiten zum antworten
        //wenn measurement und device id passen, absteigend sortieren nach datum, limit an datensätzen, anhand von data transfer object auslesen und als liste zurückgeben;
        var measurements = await _db.Measurements.Where(m => m.MeasurementTypeId == measurementType.Id && m.DeviceId == device.Id)
            .OrderByDescending(m => m.Timestamp)
            .Take(limit)
            .Select(m => new MeasurementResponseDto
            {
                Timestamp = m.Timestamp,
                Value = m.Value,
                Unit = m.MeasurementType.Unit
            })
            .ToListAsync();

        //return des measurements an client
        return Ok(measurements);
    }



    //POST 
    //diese Methode reagiert auf httpPost anfragen; 
    //task (wie promise) als asyncroner funkitons rückgabewert mit actionresult als komplettes http status objekt
    [HttpPost]
    public async Task<ActionResult> CreateMeasurement([FromBody] CreateMeasurementDto dto)
    {
        //async aus der db auslesen und erstes match mit deviceidentifier oder null bei keinem match auslesen;
        var device = await _db.Devices.FirstOrDefaultAsync(d => d.DeviceIdentifier == dto.DeviceIdentifier);

        //wenn null, Fehlermeldung antworten;
        if (device is null) return BadRequest($"Unknown Device-Identifier: {dto.DeviceIdentifier}");

        //auslesen aus db und vergleich ob request MeasurementTypeName mit db Name matcht oder null; 
        var measurementType = await _db.MeasurementTypes.FirstOrDefaultAsync(m => m.Name == dto.MeasurementTypeName);

        //Fehlermeldung bei null
        if (measurementType is null) return BadRequest($"Unknown Measurement-Type: {dto.MeasurementTypeName}");

        //nun aus dem body ein neues Measurement entity object initialisieren zum speichern
        var measurement = new Measurement
        {
            DeviceId = device.Id,
            MeasurementTypeId = measurementType.Id,
            Value = dto.Value,
            Timestamp = dto.Timestamp
        };

        //asynchrones speichern des measurements in die db
        //try catch wird bereits vom framework gehandelt für db zugriffe wie speichern, kein try catch nötig
        _db.Measurements.Add(measurement);

        // Ein eingehender Messwert ist zugleich ein Lebenszeichen des Geräts.
        // Ohne das bliebe LastSeen für den Pi ewig null (nur ProcessCommandResult setzt es,
        // und der Pi selbst führt keine Schaltbefehle aus) -> Dashboard immer "offline".
        device.LastSeen = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        //wenn erfolgreich, 201 saved zurückgeben; 
        return StatusCode(201);

    }

}
