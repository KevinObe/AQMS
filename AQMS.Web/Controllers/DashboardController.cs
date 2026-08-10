using AQMS.Web.Data;
using AQMS.Web.Models;
using AQMS.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AQMS.Web.Controllers;

// Kein [ApiController] — das ist ein MVC-Controller, der Views rendert.
// [Authorize] auf Klassenebene: die ganze Seite ist nur eingeloggt erreichbar.
[Authorize]
public class DashboardController : Controller
{
    private readonly AqmsDbContext _db;
    private readonly CommandService _commandService;
    private readonly UserManager<IdentityUser> _userManager;

    public DashboardController(
        AqmsDbContext db,
        CommandService commandService,
        UserManager<IdentityUser> userManager)
    {
        _db = db;
        _commandService = commandService;
        _userManager = userManager;
    }

    // Zeitzone einmal statisch: Linux/Debian kennt die IANA-ID "Europe/Vienna",
    // Windows kennt sie ab .NET 6
    private static readonly TimeZoneInfo Wien =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Vienna");

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Nur die schaltbaren Geräte (SmartPlug), der Pi selbst gehört nicht in die Toggle-Liste
        var devices = await _db.Devices
            .Where(d => d.DeviceType.Name == "SmartPlug" && d.IsEnabled)
            .OrderBy(d => d.Name)
            .Select(d => new DeviceRow
            {
                Id = d.Id,
                Name = d.Name,
                CurrentState = d.CurrentState,
                LastSwitchedAt = d.LastSeen,
                HasPendingCommand = d.Commands.Any(c => c.Status == CommandStatus.Pending)
            })
            .ToListAsync();

        // Temperaturverlauf: die letzten 50 Werte des Pi
        var points = await _db.Measurements
            .Where(m => m.Device.DeviceIdentifier == "raspberry-pi"
                     && m.MeasurementType.Name == "Temperature")
            .OrderByDescending(m => m.Timestamp)   // neueste zuerst, damit Take(50) die JÜNGSTEN nimmt
            .Take(50)
            .Select(m => new { m.Timestamp, m.Value })
            .ToListAsync();

        points.Reverse();   // fürs Chart wieder chronologisch

        // Pi-Online: gibt es einen Messwert aus den letzten 5 Minuten?
        // (Kadenz ist MeasurementIntervalSeconds; 5 min ist ein grosszügiger Puffer,
        //  der einen einzelnen verlorenen POST nicht sofort als Ausfall meldet.)
        var lastTs = points.LastOrDefault()?.Timestamp;

        var vm = new DashboardViewModel
        {
            Devices = devices,
            ChartLabels = points
                .Select(p => TimeZoneInfo.ConvertTimeFromUtc(p.Timestamp, Wien).ToString("HH:mm"))
                .ToList(),
            ChartValues = points.Select(p => p.Value).ToList(),
            CurrentTemperature = points.LastOrDefault()?.Value,
            LastMeasurementAt = lastTs,
            PiOnline = lastTs.HasValue && lastTs > DateTime.UtcNow.AddMinutes(-5)
        };

        return View(vm);
    }

    // Schalten darf nur Admin. ValidateAntiForgeryToken schützt gegen CSRF —
    // ohne das könnte eine fremde Seite den eingeloggten Browser zum Schalten bringen.
    [HttpPost]
    [Authorize(Roles = IdentitySeeder.AdminRole)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int deviceId, DeviceState action)
    {
        var userId = _userManager.GetUserId(User);

        var result = await _commandService.CreateCommandAsync(deviceId, action, userId);

        // TempData überlebt genau einen Redirect (Post/Redirect/Get-Pattern:
        // verhindert doppeltes Schalten beim Neuladen der Seite).
        TempData["Meldung"] = result switch
        {
            CreateCommandResult.Success => $"Befehl '{action}' wurde eingereiht.",
            CreateCommandResult.DeviceNotFound => "Gerät nicht gefunden.",
            CreateCommandResult.DeviceDisabled => "Gerät ist deaktiviert.",
            CreateCommandResult.AlreadyPending => "Für dieses Gerät ist bereits ein Befehl offen.",
            _ => "Unbekannter Fehler."
        };

        return RedirectToAction(nameof(Index));
    }
}