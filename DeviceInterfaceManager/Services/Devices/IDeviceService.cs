using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;

namespace DeviceInterfaceManager.Services.Devices;

public interface IDeviceService : IInputService, IOutputService
{
    public string? Id { get; }
    public string? DeviceName { get; }
    public Geometry? Icon { get; }

    public Task<ConnectionStatus> ConnectAsync(CancellationToken cancellationToken);
    public void Disconnect();
}

public enum ConnectionStatus
{
    Default,
    NotConnected,
    PingSuccessful,
    Connected
}