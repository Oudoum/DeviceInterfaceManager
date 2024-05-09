using System.Threading;
using System.Threading.Tasks;

namespace DeviceInterfaceManager.Models.FlightSim;

public interface IFlightSimConnection
{
    bool IsConnected { get; }
    
    Task ConnectAsync(CancellationToken cancellationToken);

    void Disconnect();
}