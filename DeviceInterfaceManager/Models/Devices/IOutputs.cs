namespace DeviceInterfaceManager.Models.Devices;

public interface IOutputs
{
    public ComponentInfo Led { get; }
    public ComponentInfo Dataline { get; }
    public ComponentInfo SevenSegment { get; }
    public ComponentInfo Analog { get; }
}