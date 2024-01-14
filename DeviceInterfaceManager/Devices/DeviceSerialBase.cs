using System;
using System.Threading.Tasks;

namespace DeviceInterfaceManager.Devices;

public class DeviceSerialBase : IDeviceSerial
{
    public ComponentInfo Switch { get; } = new(1, 255);
    public event EventHandler<InputChangedEventArgs>? InputChanged;
    public ComponentInfo Led { get; } = new(1, 255);
    public ComponentInfo Dataline { get; } = new(1, 255);
    public ComponentInfo SevenSegment { get; } = new(1, 255);
    public Task SetLedAsync(string position, bool isEnabled)
    {
        return Task.CompletedTask;
    }

    public Task SetDatalineAsync(string position, bool isEnabled)
    {
        return Task.CompletedTask;
    }

    public Task SetSevenSegmentAsync(string position, string data)
    {
        return Task.CompletedTask;
    }

    public string BoardName => "Debug";
    public string SerialNumber => "000000";

    public Task<ConnectionStatus> ConnectAsync()
    {
        return Task.FromResult(ConnectionStatus.Connected);
    }

    public void Disconnect()
    {

    }
}