using System.Text.Json.Serialization;

namespace DeviceInterfaceManager.Models;

[JsonDerivedType(typeof(Connection), typeDiscriminator: nameof(Connection))]
[JsonDerivedType(typeof(FsCockpitConnection), typeDiscriminator: nameof(FsCockpitConnection))]
public interface IConnection
{
    public string? DriverName { get; set; }
    
    public string? ConnectionName { get; set; }
}