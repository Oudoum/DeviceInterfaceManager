using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.Services.Devices;

public class DeviceSerialService : DeviceServiceBase
{
    public DeviceSerialService()
    {
        Id = "00000";
        DeviceName = "Debug";
        Icon = (Geometry?)Application.Current!.FindResource("UsbPort");
        Inputs = new Inputs.Builder().SetSwitchInfo(1, 256).SetAnalogInfo(1, 8).Build();
        Outputs = new Outputs.Builder().SetLedInfo(1, 256).SetDatalineInfo(1, 256).SetSevenSegmentInfo(1, 256).SetAnalogInfo(1, 1).Build();
    }

    public override Task SetLedAsync(int position, bool isEnabled)
    {
        return Task.CompletedTask;
    }

    public override Task SetDatalineAsync(int position, bool isEnabled)
    {
        return Task.CompletedTask;
    }

    public override Task SetSevenSegmentAsync(int position, string data)
    {
        return Task.CompletedTask;
    }

    public override Task SetAnalogAsync(int position, double value)
    {
        return Task.CompletedTask;
    }

    public override Task ResetAllOutputsAsync()
    {
        return Task.CompletedTask;
    }

    public override Task<ConnectionStatus> ConnectAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(ConnectionStatus.Connected);
    }

    public override void Disconnect()
    {
        
    }
}