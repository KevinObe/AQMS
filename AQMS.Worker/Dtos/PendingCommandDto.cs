
namespace AQMS.Worker.Dtos;

internal class PendingCommandDto
{
    public long CommandId { get; set; }

    public string Action { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string DeviceIdentifier { get; set; } = string.Empty;
    public string? IPAddress { get; set; }
}
