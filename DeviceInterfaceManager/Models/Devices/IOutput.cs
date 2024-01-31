using System.Threading.Tasks;

namespace DeviceInterfaceManager.Models.Devices;

public interface IOutput
{
    ComponentInfo Led { get; }
    ComponentInfo Dataline { get; }
    ComponentInfo SevenSegment { get; }
    
    Task SetLedAsync(string position, bool isEnabled);
    
    Task SetDatalineAsync(string position, bool isEnabled);
    
    Task SetSevenSegmentAsync(string position, string data);
}