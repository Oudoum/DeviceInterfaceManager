using Microsoft.AspNetCore.SignalR;

namespace DeviceInterfaceManager.Server;

public class DataHub : Hub
{
    public async Task SendTitle(string? message)
    {
        await Clients.All.SendAsync(nameof(SendTitle), message);
    }

    public async Task SendPmdgData(byte id, byte[] message)
    {
        await Clients.All.SendAsync(nameof(SendPmdgData), id, message);
    }
    
    public override async Task OnConnectedAsync()
    {
        await SendConnected();
        await base.OnConnectedAsync();
    }

    public async Task SendConnected()
    {
        await Clients.All.SendAsync(nameof(SendConnected), Context.ConnectionId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}