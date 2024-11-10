using System;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using DeviceInterfaceManager.Models.FlightSim.MSFS.PMDG.SDK;
using DeviceInterfaceManager.Server;

namespace DeviceInterfaceManager.Models;

public class SignalRClientService
{
    private HubConnection? _connection;
    public event Action? Connected;

    public async Task StartConnectionAsync(string? ipAddress, int? port, CancellationToken cancellationToken)
    {
        if (!IPAddress.TryParse(ipAddress, out IPAddress? address))
        {
            address = IPAddress.Loopback;
        }

        port ??= 2024;

        _connection = new HubConnectionBuilder()
            .WithUrl($"http://{address}:{port}/datahub")
            .WithAutomaticReconnect()
            .AddMessagePackProtocol()
            .Build();

        _connection.On<string?>(nameof(DataHub.SendConnected), OnConnected);

        try
        {
            await _connection.StartAsync(cancellationToken);
        }
        catch (Exception)
        {
            // ignored
        }
    }

    public async Task StopConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection is null)
        {
            return;
        }

        try
        {
            await _connection.StopAsync(cancellationToken);
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private void OnConnected(string? connectionId)
    {
        if (_connection?.ConnectionId != connectionId)
        {
            Connected?.Invoke();
        }
    }

    private async Task SendMessageAsync(string methodName, string? message, CancellationToken cancellationToken = default)
    {
        if (_connection is null)
        {
            return;
        }

        try
        {
            await _connection.InvokeAsync(methodName, message, cancellationToken);
        }
        catch (Exception)
        {
            // ignored
        }
    }

    public async Task SendPmdgDataMessageAsync(byte id, byte[] message)
    {
        if (_connection is null)
        {
            return;
        }

        try
        {
            await _connection.InvokeAsync(nameof(DataHub.SendPmdgData), id, message);
        }
        catch (Exception)
        {
            // ignored
        }
    }

    public async Task SendTitleMessageAsync(string? title)
    {
        await SendMessageAsync(nameof(DataHub.SendTitle), title);
    }

    public static byte[] CreateCduTestData()
    {
        Cdu.Screen.Row.Cell cell = new() { Symbol = (byte)'T' };
        var cells = Enumerable.Repeat(cell, 14).ToArray();
        Cdu.Screen.Row row = new() { Rows = cells };
        var rows = Enumerable.Repeat(row, 24).ToArray();
        Cdu.Screen screen = new() { Columns = rows, Powered = true };

        int size = Marshal.SizeOf(screen);
        byte[] arr = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);

        try
        {
            Marshal.StructureToPtr(screen, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }

        return arr;
    }
}