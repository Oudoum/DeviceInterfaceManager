using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace DeviceInterfaceManager.Models.Devices;

public class DeviceSerialBase : IDeviceSerial
{
    public ComponentInfo Switch { get; } = new(1, 256);
    public ComponentInfo AnalogIn { get; } = new(1, 8);
    public event EventHandler<SwitchPositionChangedEventArgs>? SwitchPositionChanged;
    public event EventHandler<AnalogInValueChangedEventArgs>? AnalogInValueChanged;
    public ComponentInfo Led { get; } = new(1, 256);
    public ComponentInfo Dataline { get; } = new(1, 256);
    public ComponentInfo SevenSegment { get; } = new(1, 256);
    public ComponentInfo AnalogOut { get; } = new(1, 1);
    public Task SetLedAsync(string? position, bool isEnabled)
    {
        return Task.CompletedTask;
    }

    public Task SetDatalineAsync(string? position, bool isEnabled)
    {
        return Task.CompletedTask;
    }

    public Task SetSevenSegmentAsync(string? position, string data)
    {
        return Task.CompletedTask;
    }

    public Task SetAnalogAsync(string? position, int value)
    {
        return Task.CompletedTask;
    }

    public Task ResetAllOutputsAsync()
    {
        return Task.CompletedTask;
    }

    public string Id => "000000";
    public string DeviceName => "Debug";
    public Geometry? Icon { get; } = (Geometry?)Application.Current!.FindResource("UsbPort");

    public Task<ConnectionStatus> ConnectAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(ConnectionStatus.Connected);
    }

    public void Disconnect()
    {

    }

    private void OnSwitchPositionChanged(SwitchPositionChangedEventArgs e)
    {
        SwitchPositionChanged?.Invoke(this, e);
    }

    private void OnAnalogInChanged(AnalogInValueChangedEventArgs e)
    {
        AnalogInValueChanged?.Invoke(this, e);
    }
}