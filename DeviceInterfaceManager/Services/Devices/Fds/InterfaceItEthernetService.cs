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
using Microsoft.Extensions.Logging;

namespace DeviceInterfaceManager.Services.Devices.Fds;

public class InterfaceItEthernetService : DeviceServiceBase
{
    private readonly ILogger _logger;

    public InterfaceItEthernetService(string? iPAddress, ILogger logger)
    {
        _logger = logger;
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
        catch (ObjectDisposedException e)
        {
            _logger.LogError(e, "[{Id}] Network stream was disposed.", Id);
        }
        catch (InvalidOperationException e)
        {
            _logger.LogError(e, "[{Id}] Invalid operation occurred.", Id);
        }
        catch (IOException e)
        {
            _logger.LogError(e, "[{Id}] I/O error occurred.", Id);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("[{Id}] Operation was canceled.", Id);
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

    public override async Task Disconnect()
    {
        await CloseStream();
    }

    private const int TcpPort = 10346;

    private TcpClient? _tcpClient;

    private NetworkStream? _networkStream;

    public static async Task<List<string>> ReceiveControllerDiscoveryDataAsync(ILogger logger)
    {
        using UdpClient client = new();
        client.EnableBroadcast = true;
        client.Send("D"u8, new IPEndPoint(IPAddress.Broadcast, 30303));

        List<string> responses = [];
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(3));

        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
                var receiveTask = client.ReceiveAsync(cts.Token).AsTask();
                Task completedTask = await Task.WhenAny(receiveTask, Task.Delay(500, cts.Token));

                if (completedTask != receiveTask)
                {
                    continue;
                }

                UdpReceiveResult result = await receiveTask;
                responses.Add(result.RemoteEndPoint.Address.ToString());
            }
        }
        catch (SocketException e)
        {
            logger.LogError(e, "Network issue occurred.");
        }
        catch (ObjectDisposedException e)
        {
            logger.LogError(e, "UdpClient was disposed.");
        }
        catch (IOException e)
        {
            logger.LogError(e, "I/O error occurred.");
        }

        return responses;
    }

    private async Task<bool> PingHostAsync()
    {
        if (Id is null)
        {
            _logger.LogWarning("Connection attempt failed: Id is null.");
            return false;
        }

        using Ping ping = new();
        return (await ping.SendPingAsync(Id)).Status == IPStatus.Success;
    }

    private async Task<bool> ConnectToHostAsync(CancellationToken cancellationToken)
    {
        if (Id is null)
        {
            _logger.LogWarning("Connection attempt failed: Id is null.");
            return false;
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(Id, TcpPort, cancellationToken);
                _networkStream = _tcpClient.GetStream();
                await GetInterfaceItEthernetDataAsync(cancellationToken);
                _logger.LogInformation("[{Id}] Successfully connected to host.", Id);
                return true;
            }
            catch (ArgumentNullException e)
            {
                _logger.LogError(e, "[{Id}] ArgumentNullException: Invalid connection parameters.", Id);
                await CloseStream();
                return false;
            }
            catch (SocketException e)
            {
                _logger.LogError(e, "[{Id}] SocketException: Failed to connect to host.", Id);
                await CloseStream();
                return false;
            }
            catch (OperationCanceledException)
            {
                await CloseStream();
                return false;
            }
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
                catch (ObjectDisposedException e)
                {
                    _logger.LogError(e, "[{Id}] Network stream was disposed.", Id);
                    return;
                }
                catch (InvalidOperationException e)
                {
                    _logger.LogError(e, "[{Id}] Invalid operation occurred.", Id);
                    return;
                }
                catch (IOException e)
                {
                    _logger.LogError(e, "[{Id}] I/O error occurred.", Id);
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
                            _logger.LogInformation("[{Id}] State two reached.", Id);
                            isInitializing = true;
                            break;

                        case "STATE=3":
                            _logger.LogInformation("[{Id}] State three reached.", Id);
                            isSwitchIdentifying = true;
                            break;

                        case "STATE=4":
                            _logger.LogInformation("[{Id}] State four reached.", Id);
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

    private void GetInterfaceItEthernetInfoData(Inputs.Builder inputsBuilder, Outputs.Builder outputsBuilder, string ethernetData)
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

    private static void GetConfigData(Inputs.Builder inputsBuilder, Outputs.Builder outputsBuilder, string value)
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

    private static ComponentInfo GetComponentInfo(string[] config)
    {
        return new ComponentInfo(Convert.ToInt32(config[3]), Convert.ToInt32(config[5]));
    }

    private async Task CloseStream()
    {
        try
        {
            await ResetAllOutputsAsync();
            if (_networkStream is not null)
            {
                await _networkStream.WriteAsync(Encoding.ASCII.GetBytes("DISCONNECT" + "\r\n"));
            }
        }
        catch (ObjectDisposedException e)
        {
            _logger.LogError(e, "[{Id}] Network stream was disposed.", Id);
        }
        catch (InvalidOperationException e)
        {
            _logger.LogError(e, "[{Id}] Invalid operation occurred.", Id);
        }
        catch (IOException e)
        {
            _logger.LogError(e, "[{Id}] I/O error occurred.", Id);
        }
        finally
        {
            if (_networkStream is not null)
            {
                await _networkStream.DisposeAsync();
            }

            _tcpClient?.Close();
        }
    }
}