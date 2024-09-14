using System.Threading.Tasks;

namespace DeviceInterfaceManager.Models.Devices;

public interface IOutput
{
    public ComponentInfo Led { get; }
    public ComponentInfo Dataline { get; }
    public ComponentInfo SevenSegment { get; }
    public ComponentInfo AnalogOut { get; }

    public Task SetLedAsync(int position, bool isEnabled);

    public Task SetDatalineAsync(int position, bool isEnabled);

    public Task SetSevenSegmentAsync(int position, string data);

    public Task SetAnalogAsync(int position, int value);

    public Task ResetAllOutputsAsync();
}