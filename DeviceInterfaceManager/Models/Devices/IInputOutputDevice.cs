using System.Threading.Tasks;
using Avalonia.Media;

namespace DeviceInterfaceManager.Models.Devices;

public interface IInputOutputDevice : IInput, IOutput
{
    string? DeviceName { get; }
    string? SerialNumber { get; }
    Geometry? Icon { get; }

    Task<ConnectionStatus> ConnectAsync();
    void Disconnect();
}

public enum ConnectionStatus
{
    Default,
    NotConnected,
    PingSuccessful,
    Connected
}