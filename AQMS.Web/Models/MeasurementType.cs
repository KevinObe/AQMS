namespace AQMS.Web.Models;

public class MeasurementType
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Unit { get; set; } = string.Empty;

    public ICollection<Measurement> Measurements { get; set; } = new List<Measurement>();

}
