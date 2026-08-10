namespace AQMS.Web.Models;

public class DeviceType
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    //Interface-basierte Navigation-Properties - eine Liste aber Interface convention in C#
    public ICollection<Device> Devices { get; set; } = new List<Device>();
}
