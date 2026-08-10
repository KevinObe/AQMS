namespace AQMS.Web.Dtos;

//DTO für Befehle welche erst abgerufen werden müssen; 
//Response für GET requests
//kein deviceId notwendig hier, worker fragt gezielt nach 
//status auch nicht wichtig, sind alle status pending; 

public class CommandPendingDto
{
    public long CommandId { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Geräte Identifier und die LAN-IP (für den direkten Schalt-Call)
    public string DeviceIdentifier { get; set; } = string.Empty;
    public string? IPAddress { get; set; }
}