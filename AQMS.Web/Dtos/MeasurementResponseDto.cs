namespace AQMS.Web.Dtos;
//DTO für Get request bzw antwort auf GET requests
public class MeasurementResponseDto
{
    public DateTime Timestamp { get; set; }

    public double Value { get; set; }

    public string Unit { get; set; } = string.Empty;
}
