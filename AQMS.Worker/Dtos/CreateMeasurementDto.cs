namespace AQMS.Worker.Dtos;

internal class CreateMeasurementDto
{
    //DeviceIdentifier — string
    public string DeviceIdentifier { get; set; } = string.Empty;

    //MeasurementTypeName — string
    public string MeasurementTypeName {  get; set; } = string.Empty;

    //Value — double
    public double Value { get; set; }

    //Timestamp — DateTime
    public DateTime Timestamp { get; set; }
}
