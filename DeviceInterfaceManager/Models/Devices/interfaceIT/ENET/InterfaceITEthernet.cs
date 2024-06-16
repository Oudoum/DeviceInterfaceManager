using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace DeviceInterfaceManager.Models.Devices.interfaceIT.ENET;

public class InterfaceItEthernet(string iPAddress) : IInputOutputDevice
{
    public ComponentInfo Switch { get; private set; } = new(0,0);
    public event EventHandler<InputChangedEventArgs>? InputChanged;
    public ComponentInfo Led { get; private set; } = new(0,0);
    public ComponentInfo Dataline { get; private set; } = new(0,0);
    public ComponentInfo SevenSegment { get; private set; } = new(0,0);

    public async Task SetLedAsync(string? position, bool isEnabled)
    {
        try
        {
            if (_networkStream is not null)
            {
                await _networkStream.WriteAsync(Encoding.ASCII.GetBytes("B1:LED:" + position + ":" + Convert.ToUInt16(isEnabled) + "\r\n"));
            }
        }
        catch (Exception)
        {
            // ignored
        }
    }

    public Task SetDatalineAsync(string? position, bool isEnabled)
    {
        //Add
        return Task.CompletedTask;
    }

    public Task SetSevenSegmentAsync(string? position, string data)
    {
        //Add
        return Task.CompletedTask;
    }

    public async Task ResetAllOutputsAsync()
    {
        await Led.PerformOperationOnAllComponents(async i => await SetLedAsync(i, false));
        await Dataline.PerformOperationOnAllComponents(async i => await SetDatalineAsync(i, false));
        await SevenSegment.PerformOperationOnAllComponents(async i => await SetSevenSegmentAsync(i, " "));
    }

    public string Id { get; } = iPAddress;
    public string? DeviceName { get; private set; }
    public Geometry? Icon { get; } = (Geometry?)Application.Current!.FindResource("Ethernet");

    public async Task<ConnectionStatus> ConnectAsync(CancellationToken cancellationToken)
    {
        if (!await PingHostAsync())
        {
            return ConnectionStatus.NotConnected;
        }

        if (!await ConnectToHostAsync(cancellationToken))
        {
            return ConnectionStatus.PingSuccessful;
        }

        return ConnectionStatus.Connected;
    }

    public async void Disconnect()
    {
       await CloseStream();
    }

    private const int TcpPort = 10346;

    private TcpClient? _tcpClient;

    private NetworkStream? _networkStream;
    
    public static async Task<string> ReceiveControllerDiscoveryDataAsync()
    {
        UdpClient client = new() { EnableBroadcast = true };
        client.Send("D"u8, new IPEndPoint(IPAddress.Broadcast, 30303));
        try
        {
            UdpReceiveResult result = await client.ReceiveAsync().WaitAsync(TimeSpan.FromSeconds(1));
            return result.RemoteEndPoint.Address.ToString();
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    private async Task<bool> PingHostAsync()
    {
        using Ping ping = new();
        return (await ping.SendPingAsync(Id)).Status == IPStatus.Success;
    }

    private async Task<bool> ConnectToHostAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(Id, TcpPort, cancellationToken);
                _networkStream = _tcpClient.GetStream();
                await GetInterfaceItEthernetDataAsync(cancellationToken);
                return true;
            }
            catch (OperationCanceledException)
            {
                await CloseStream();
                return false;
            }
            catch (ArgumentNullException)
            {
                await CloseStream();
                return false;
            }
            catch (SocketException)
            {
                await CloseStream();
                return false;
            }

        return false;
    }

    private async Task GetInterfaceItEthernetDataAsync(CancellationToken cancellationToken)
    {
        TaskCompletionSource tcs = new();
        _ = Task.Run(async () =>
        {
            StringBuilder sb = new();
            byte[] buffer = new byte[8192];
            bool isInitializing = false;
            bool isSwitchIdentifying = false;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (_networkStream is not null)
                    {
                        int bytesRead = await _networkStream.ReadAsync(buffer, cancellationToken);
                        sb.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));
                        if (buffer[bytesRead - 1] != 10)
                        {
                            continue;
                        }
                    }
                }
                catch (IOException)
                {
                    return;
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                foreach (string ethernetData in sb.ToString().Split("\r\n", StringSplitOptions.RemoveEmptyEntries))
                {
                    switch (ethernetData)
                    {
                        case "STATE=2":
                            isInitializing = true;
                            break;

                        case "STATE=3":
                            isSwitchIdentifying = true;
                            
                            break;

                        case "STATE=4":
                            isInitializing = false;
                            isSwitchIdentifying = false;
                            break;

                        default:
                            if (isSwitchIdentifying || !isInitializing)
                            {
                                ProcessSwitchData(ethernetData);
                            }
                            else if (isInitializing && !isSwitchIdentifying)
                            {
                                GetInterfaceItEthernetInfoData(ethernetData);
                                if (!tcs.Task.IsCompleted)
                                {
                                    tcs.SetResult();
                                }
                            }

                            break;
                    }
                }

                sb.Clear();
            }
        }, cancellationToken);
        
        await tcs.Task;
    }

    private void ProcessSwitchData(string ethernetData)
    {
        if (!ethernetData.StartsWith("B1="))
        {
            return;
        }

        string[] splitSwitchData = ethernetData.Replace("B1=SW:", string.Empty).Split(':');

        if (!int.TryParse(splitSwitchData[0], out int position))
        {
            return;
        }

        bool isPressed = splitSwitchData[1] == "ON";

        Switch.UpdatePosition(position, isPressed);
        InputChanged?.Invoke(this, new InputChangedEventArgs(position, isPressed));
    }

    private void GetInterfaceItEthernetInfoData(string ethernetData)
    {
        int index = ethernetData.IndexOf('=');
        if (index < 0)
        {
            return;
        }

        string value = ethernetData[(index + 1)..];
        switch (ethernetData[..index])
        {
            case "NAME":
                DeviceName = value;
                break;

            case "CONFIG":
                GetConfigData(value);
                break;
        }
    }

    private void GetConfigData(string value)
    {
        string[] config = value.Split(":");
        
        switch (config[1])
        {
            case "LED":
                Led = GetComponentInfo(config);
                break;

            case "SWITCH":
                Switch = GetComponentInfo(config);
                break;

            case "7 SEGMENT":
                SevenSegment = GetComponentInfo(config);
                break;

            case "DATALINE":
                Dataline = GetComponentInfo(config);
                break;

            case "ENCODER":
                //Add
                break;

            case "ANALOG IN":
                //Add
                break;

            case "PULSE WIDTH":
                //Add
                break;
        }
    }

    private static ComponentInfo GetComponentInfo(IReadOnlyList<string> config)
    {
        return new ComponentInfo(Convert.ToInt32(config[3]), Convert.ToInt32(config[5]));
    }

    private async Task CloseStream()
    {
        try
        {
            await ResetAllOutputsAsync();
            _networkStream?.Write(Encoding.ASCII.GetBytes("DISCONNECT" + "\r\n"));
            _tcpClient?.Close();
        }
        catch (Exception)
        {
            // ignored
        }
    }
}