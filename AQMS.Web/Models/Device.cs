using System.ComponentModel.DataAnnotations.Schema;

namespace AQMS.Web.Models;
public class Device
{
    public int Id { get; set; }

    //empty string als default wert
    public string Name { get; set; } = string.Empty;

    public string DeviceIdentifier { get; set; } = string.Empty;

    public string? IPAddress { get; set; }

    public DeviceState? CurrentState { get; set; }

    public bool IsEnabled { get; set; } = true;

    public DateTime? LastSeen { get; set; }

    //Fremdschlüssel zu DeviceType
    public int DeviceTypeId { get; set; }

    //null! = Null-Forgiving Operator -> compiler erwartet default wert, bei null! wird eine art promise gegeben, dass der wert gesetzt wird und nicht null wenn ich die Property tatsächlich verwende (lese)
    public DeviceType DeviceType { get; set; } = null!;

    public ICollection<Measurement> Measurements { get; set; } = new List<Measurement>();

    public ICollection<DeviceCommand> Commands { get; set; } = new List<DeviceCommand>();

    //not mapped -> nicht in db gespeichert
    //berechneter Wert -> wenn last seen einen wert hat und wenn lastseen größer ist als die jetzige zeit -30 sekunden, dann true; 
    [NotMapped]
    public bool IsOnline => LastSeen.HasValue && LastSeen > DateTime.UtcNow.AddSeconds(-30);

    public ICollection<StateChange> StateChanges { get; set; } = new List<StateChange>();


}

public enum DeviceState
{
    Off,
    On
}
