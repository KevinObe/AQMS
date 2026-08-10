namespace AQMS.Web.Models;

public class Measurement
{
    //long integer da sich extrem viele Daten häufen können
    public long Id { get; set; }

    public DateTime Timestamp { get; set; }

    // kommazahlen bei den Werten, bsp. 24.5 Grad
    public double Value { get; set; }

    public int MeasurementTypeId { get; set; }

    public int DeviceId { get; set; }

    //hier keine Listen oder ICollections mehr, weil diese die n Seite der Beziehung darstellen; 
    //Es gibt in Measurement auch keine Children mehr die in die Listen kommen würden;
    public Device Device { get; set; } = null!;

    public MeasurementType MeasurementType { get; set; } = null!;


}
