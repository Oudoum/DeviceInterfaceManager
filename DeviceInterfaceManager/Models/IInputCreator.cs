using DeviceInterfaceManager.Models.Devices;
using DeviceInterfaceManager.Models.FlightSim.MSFS.PMDG.SDK;

namespace DeviceInterfaceManager.Models;

public interface IInputCreator
{
    public string? InputType { get; set; }

    public Component? Input { get; set; }

    public string? EventType { get; set; }

    public int? PmdgEvent { get; set; }

    public string? Event { get; set; }

    public bool OnRelease { get; set; }

    public Mouse? PmdgMousePress { get; set; }

    public Mouse? PmdgMouseRelease { get; set; }

    public uint? DataPress { get; set; }
    
    public uint? DataPress2 { get; set; }

    public uint? DataRelease { get; set; }
    
    public uint? DataRelease2 { get; set; }
}