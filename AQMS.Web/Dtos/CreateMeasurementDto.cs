namespace AQMS.Web.Dtos;

//DTO - Data Transfer Object - klasse die nicht in der db abgebildet ist mit genauem aufbau
//verhindert dass client daten oder gar ids sendet, verhindert daten die inkonsistent werden oder id konflikte
//das DTO gibt exakt vor welche werte gebraucht werden und diese werden über die API gesendet
//hier kommt keine logik, es dient rein als datenmodell
public class CreateMeasurementDto
{
    public string DeviceIdentifier { get; set; } = string.Empty;

    public string MeasurementTypeName { get; set; } = string.Empty;

    public double Value { get; set; }

    public DateTime Timestamp { get; set; }
}



