using System.Threading.Tasks;
using Avalonia.Media;

namespace DeviceInterfaceManager.Models.Devices;

public interface IInputOutputDevice : IInput, IOutput
{
    public string? Id { get; }
    public string? DeviceName { get; }
    public Geometry? Icon { get; }

    public Task<ConnectionStatus> ConnectAsync();
    public void Disconnect();
}

public enum ConnectionStatus
{
    Default,
    NotConnected,
    PingSuccessful,
    Connected
}