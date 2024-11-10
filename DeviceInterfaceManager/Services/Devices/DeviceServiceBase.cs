using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.Services.Devices;

public abstract class DeviceServiceBase  : IDeviceService
{
    public Inputs? Inputs { get; protected set; }
    public event EventHandler<SwitchPositionChangedEventArgs>? SwitchPositionChanged;
    public event EventHandler<AnalogValueChangedEventArgs>? AnalogValueChanged;
    public Outputs? Outputs { get; protected set; }
    public abstract Task SetLedAsync(int position, bool isEnabled);

    public abstract Task SetDatalineAsync(int position, bool isEnabled);

    public abstract Task SetSevenSegmentAsync(int position, string data);

    public abstract Task SetAnalogAsync(int position, double value);

    public virtual async Task ResetAllOutputsAsync()
    {
        if (Outputs is null)
        {
            return;
        }
        
        await Outputs.Led.PerformOperationOnAllComponents(async i => await SetLedAsync(i, false));
        await Outputs.Dataline.PerformOperationOnAllComponents(async i => await SetDatalineAsync(i, false));
        await Outputs.SevenSegment.PerformOperationOnAllComponents(async i => await SetSevenSegmentAsync(i, " "));
    }

    public string? Id { get; protected set; }
    public string? DeviceName { get; protected set; }
    public Geometry? Icon { get; protected set; }

    public abstract Task<ConnectionStatus> ConnectAsync(CancellationToken cancellationToken);

    public abstract void Disconnect();

    protected void OnSwitchPositionChanged(int position, bool isPressed)
    {
        Inputs?.Switch.UpdatePosition(position, isPressed);
        SwitchPositionChanged?.Invoke(this, new SwitchPositionChangedEventArgs(position, isPressed));
    }

    protected void OnAnalogInValueChanged(int position, int value)
    {
        Inputs?.Analog.UpdatePosition(position, value);
        AnalogValueChanged?.Invoke(this, new AnalogValueChangedEventArgs(position, value));
    }
}