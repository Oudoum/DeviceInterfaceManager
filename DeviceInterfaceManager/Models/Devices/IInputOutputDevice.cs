using System.Threading.Tasks;

namespace DeviceInterfaceManager.Models.Devices;

public interface IInputOutputDevice : IInput, IOutput
{
    string? DeviceName { get; }
    string? SerialNumber { get; }

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