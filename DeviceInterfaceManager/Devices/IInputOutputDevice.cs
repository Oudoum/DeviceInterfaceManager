using System.Threading.Tasks;

namespace DeviceInterfaceManager.Devices;

public interface IInputOutputDevice : IInput, IOutput
{
    string? BoardName { get; }
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