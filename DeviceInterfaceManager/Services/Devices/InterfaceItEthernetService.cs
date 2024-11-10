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
using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.Services.Devices;

public class InterfaceItEthernetService : DeviceServiceBase
{
    public InterfaceItEthernetService(string iPAddress)
    {
        Id = iPAddress;
        Icon = (Geometry?)Application.Current!.FindResource("Ethernet");
    }
    public override async Task SetLedAsync(int position, bool isEnabled)
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

    public override Task SetDatalineAsync(int position, bool isEnabled)
    {
        //Add
        return Task.CompletedTask;
    }

    public override Task SetSevenSegmentAsync(int position, string data)
    {
        //Add
        return Task.CompletedTask;
    }

    public override Task SetAnalogAsync(int position, double value)
    {
        //Add
        return Task.CompletedTask;
    }

    public override async Task<ConnectionStatus> ConnectAsync(CancellationToken cancellationToken)
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

    public override async void Disconnect()
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
        if (Id is null)
        {
            return false;
        }
        
        using Ping ping = new();
        return (await ping.SendPingAsync(Id)).Status == IPStatus.Success;
    }

    private async Task<bool> ConnectToHostAsync(CancellationToken cancellationToken)
    {
        if (Id is null)
        {
            return false;
        }
        
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
        Inputs.Builder inputBuilder = new();
        Outputs.Builder outputsBuilder = new();
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
                                ProcessAnalogInData(ethernetData);
                            }
                            else if (isInitializing && !isSwitchIdentifying)
                            {
                                GetInterfaceItEthernetInfoData(inputBuilder, outputsBuilder, ethernetData);
                                Inputs = inputBuilder.Build();
                                Outputs = outputsBuilder.Build();
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

    private const string SwitchData = "B1=SW:";

    private void ProcessSwitchData(string ethernetData)
    {
        if (Inputs is null || !ethernetData.StartsWith(SwitchData))
        {
            return;
        }

        string[] splitData = ethernetData.Replace(SwitchData, string.Empty).Split(':');

        if (!int.TryParse(splitData[0], out int position))
        {
            return;
        }

        bool isPressed = splitData[1] == "ON";
        OnSwitchPositionChanged(position, isPressed);
    }

    private const string AnalogData = "B1=ANALOG:";

    private void ProcessAnalogInData(string ethernetData)
    {
        if (Inputs is null || !ethernetData.StartsWith(AnalogData))
        {
            return;
        }

        string data = ethernetData.Replace(AnalogData, string.Empty);

        if (!int.TryParse(data, out int value))
        {
            return;
        }

        OnAnalogInValueChanged(1, value);
    }

    private void GetInterfaceItEthernetInfoData(Inputs.Builder inputsBuilder,Outputs.Builder outputsBuilder, string ethernetData)
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
                GetConfigData(inputsBuilder, outputsBuilder, value);
                break;
        }
    }

    private static void GetConfigData(Inputs.Builder inputsBuilder,Outputs.Builder outputsBuilder, string value)
    {
        string[] config = value.Split(":");

        switch (config[1])
        {
            case "LED":
                outputsBuilder.SetLedInfo(GetComponentInfo(config));
                break;

            case "SWITCH":
                inputsBuilder.SetSwitchInfo(GetComponentInfo(config));
                break;

            case "7 SEGMENT":
                outputsBuilder.SetSevenSegmentInfo(GetComponentInfo(config));
                break;

            case "DATALINE":
                outputsBuilder.SetDatalineInfo(GetComponentInfo(config));
                break;

            case "ENCODER":
                //Add
                break;

            case "ANALOG IN":
                inputsBuilder.SetAnalogInfo(GetComponentInfo(config));
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