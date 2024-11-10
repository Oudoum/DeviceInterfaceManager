using System.Threading.Tasks;
using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.Services.Devices;

public interface IOutputService
{
    public Outputs? Outputs { get; }

    public Task SetLedAsync(int position, bool isEnabled);

    public Task SetDatalineAsync(int position, bool isEnabled);

    public Task SetSevenSegmentAsync(int position, string data);

    public Task SetAnalogAsync(int position, double value);

    public Task ResetAllOutputsAsync();
}