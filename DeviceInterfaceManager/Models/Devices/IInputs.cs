namespace DeviceInterfaceManager.Models.Devices;

public interface IInputs
{
    public ComponentInfo Switch { get; }
    public ComponentInfo Analog { get; }
}