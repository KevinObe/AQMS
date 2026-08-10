namespace AQMS.Web.Models;

// ViewModel statt Entity direkt an die View: die View braucht nur wenige Felder,
// und die Entity würde Navigation-Properties (und damit Lazy-Loading-Fallen) mitschleppen.
public class DashboardViewModel
{
    public List<DeviceRow> Devices { get; set; } = new();

    // Chart-Daten bewusst als fertige Strings/Doubles, nicht als DateTime:
    // die Zeitzonen-Umrechnung (UTC -> Europe/Vienna) passiert serverseitig,
    // damit der Browser nichts interpretieren muss (bekannter Z-Marker-Bug im JSON).
    public List<string> ChartLabels { get; set; } = new();
    public List<double> ChartValues { get; set; } = new();

    public double? CurrentTemperature { get; set; }
    public DateTime? LastMeasurementAt { get; set; }   // UTC
    public bool PiOnline { get; set; }
}

public class DeviceRow
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DeviceState? CurrentState { get; set; }
    public DateTime? LastSwitchedAt { get; set; }
    public bool HasPendingCommand { get; set; }
}

public class MeasurementPoint
{
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
}