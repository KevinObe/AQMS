namespace AQMS.Web.Models;

public class StateChange
{
    public long Id { get; set; }

    public DateTime Timestamp { get; set; }
    // Konsistent mit Device.CurrentState und DeviceCommand.Action
    public DeviceState State { get; set; }

    public Device Device { get; set; } = null!;

    public int DeviceId { get; set; }
    // Wer hat geschaltet — null bei automatischen Schaltvorgaengen
    public string? ChangedByUserId { get; set; }

}
