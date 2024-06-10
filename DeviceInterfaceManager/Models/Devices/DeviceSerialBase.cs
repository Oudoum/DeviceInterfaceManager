using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace DeviceInterfaceManager.Models.Devices;

public class DeviceSerialBase : IDeviceSerial
{
    public ComponentInfo Switch { get; } = new(1, 255);
    public event EventHandler<InputChangedEventArgs>? InputChanged;
    public ComponentInfo Led { get; } = new(1, 255);
    public ComponentInfo Dataline { get; } = new(1, 255);
    public ComponentInfo SevenSegment { get; } = new(1, 255);
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

    private void OnInputChanged(InputChangedEventArgs e)
    {
        InputChanged?.Invoke(this, e);
    }
}