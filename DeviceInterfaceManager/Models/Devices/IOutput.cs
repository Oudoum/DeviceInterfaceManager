using System.Threading.Tasks;

namespace DeviceInterfaceManager.Models.Devices;

public interface IOutput
{
    public ComponentInfo Led { get; }
    public ComponentInfo Dataline { get; }
    public ComponentInfo SevenSegment { get; }
    
    public Task SetLedAsync(string? position, bool isEnabled);
    
    public Task SetDatalineAsync(string? position, bool isEnabled);
    
    public Task SetSevenSegmentAsync(string? position, string data);

    public Task ResetAllOutputsAsync();
}