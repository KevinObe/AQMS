namespace AQMS.Web.Models;

public class DeviceCommand
{
    //long int weil über zeit sehr viele sein können
    public long Id { get; set; }

    public DeviceState Action { get; set; }

    public CommandStatus Status { get; set; } = CommandStatus.Pending;

    public DateTime CreatedAt { get; set; }

    //nullable da bei pending ists noch leer
    public DateTime? ExecutedAt { get; set; }

    //optionale Meldung für Feedback
    public string? ResultMessage { get; set; }

    public int DeviceId { get; set; }

    //asp.net user, IdentityPK ist ein string, es werden guids als strings genutzt
    //nullable für auto settings die nicht von usern kommen
    public string? RequestedByUserId { get; set; }

    public Device Device { get; set; } = null!;
}

public enum CommandStatus
{
    Pending,
    Executed,
    Failed
}