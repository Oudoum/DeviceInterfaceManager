using DeviceInterfaceManager.Models.Devices;
using DeviceInterfaceManager.Models.FlightSim.MSFS.PMDG;
using DeviceInterfaceManager.Models.Modifiers;

namespace DeviceInterfaceManager.Models;

public interface IInputCreator : IDescription
{
    public string? InputType { get; set; }

    public int? Input { get; set; }

    public string? EventType { get; set; }

    public string? Event { get; set; }

    public long? DataPress { get; set; }

    public long? DataPress2 { get; set; }

    public long? DataRelease { get; set; }

    public long? DataRelease2 { get; set; }

    public int? PmdgEvent { get; set; }

    public Mouse? PmdgMousePress { get; set; }

    public Mouse? PmdgMouseRelease { get; set; }

    public bool OnRelease { get; set; }
    
    public Interpolation? Interpolation { get; set; }
}