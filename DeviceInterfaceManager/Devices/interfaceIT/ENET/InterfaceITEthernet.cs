using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceInterfaceManager.Devices.interfaceIT.ENET;

public class InterfaceItEthernet(
    string iPAddress,
    TcpClient client,
    NetworkStream stream,
    InterfaceItEthernetInfo interfaceItEthernetInfo,
    bool hasErrorBeenShown)
{
    private string HostIpAddress { get; set; } = iPAddress;
    private int TcpPort { get; set; } = 10346;
    private InterfaceItEthernetInfo InterfaceItEthernetInfo { get; set; } = interfaceItEthernetInfo;
    public bool IsPolling { get; set; }

    public static async Task<InterfaceItEthernetDiscovery?> ReceiveControllerDiscoveryDataAsync()
    {
        UdpClient client = new() { EnableBroadcast = true };
        client.Send(Encoding.ASCII.GetBytes("D"), new IPEndPoint(IPAddress.Broadcast, 30303));
        try
        {
            UdpReceiveResult result = await client.ReceiveAsync().WaitAsync(TimeSpan.FromSeconds(1));
            string[] sResult = Encoding.ASCII.GetString(result.Buffer).Split("\r\n");
            InterfaceItEthernetDiscovery discovery = new()
            {
                IpAddress = result.RemoteEndPoint.Address.ToString(),
                HostName = sResult[0],
                MacAddress = sResult[1],
                Message = sResult[2],
                Id = sResult[3],
                Name = sResult[4],
                Description = sResult[5]
            };
            return discovery;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public enum ConnectionStatus
    {
        Default,
        NotConnected,
        PingSuccessful,
        Connected
    }

    public async Task<ConnectionStatus> InterfaceItEthernetConnectionAsync(CancellationToken cancellationToken)
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

    private async Task<bool> PingHostAsync()
    {
        using Ping ping = new();
        return (await ping.SendPingAsync(HostIpAddress)).Status == IPStatus.Success;
    }

    private async Task<bool> ConnectToHostAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                client = new();
                await client.ConnectAsync(HostIpAddress, TcpPort, cancellationToken);
                stream = client.GetStream();
                return true;
            }
            catch (OperationCanceledException)
            {

            }
            catch (ArgumentNullException)
            {
                CloseStream();
            }
            catch (SocketException)
            {
                CloseStream();
            }
        }
        return false;
    }

    public async Task<InterfaceItEthernetInfo> GetInterfaceItEthernetDataAsync(Action<int, uint> interfacItKeyAction, CancellationToken cancellationToken)
    {
        TaskCompletionSource<InterfaceItEthernetInfo> tcs = new();
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
                    int bytesRead = await stream.ReadAsync(buffer, cancellationToken);
                    sb.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));
                    if (buffer[bytesRead - 1] != 10)
                    {
                        continue;
                    }
                }
                catch (IOException)
                {
                    return;
                }
                catch (OperationCanceledException)
                {

                }

                foreach (string ethernetData in sb.ToString().Split("\r\n", StringSplitOptions.RemoveEmptyEntries))
                {
                    switch (ethernetData)
                    {
                        case "STATE=2":
                            isInitializing = true;
                            InterfaceItEthernetInfo = new InterfaceItEthernetInfo
                            {
                                HostIpAddress = HostIpAddress,
                            };
                            break;

                        case "STATE=3":
                            isSwitchIdentifying = true;
                            tcs.SetResult(InterfaceItEthernetInfo);
                            break;

                        case "STATE=4":
                            isInitializing = false;
                            isSwitchIdentifying = false;
                            break;

                        default:
                            if (isSwitchIdentifying || !isInitializing)
                            {
                                ProcessSwitchData(interfacItKeyAction, ethernetData);
                            }
                            else if (isInitializing && !isSwitchIdentifying)
                            {
                                GetInterfaceItEthernetInfoData(ethernetData);
                            }
                            break;
                    }
                }
                sb.Clear();
            }
        }, cancellationToken);
        return await tcs.Task;
    }

    private void ProcessSwitchData(Action<int, uint> interfaceItKeyAction, string ethernetData)
    {
        if (ethernetData.StartsWith("B1="))
        {
            string[] splitSwitchData = ethernetData.Replace("B1=SW:", string.Empty).Split(':');
            if (int.TryParse(splitSwitchData[0], out int ledNumber))
            {
                uint direction = 0;
                if (splitSwitchData[1] == "ON")
                {
                    direction = 1;
                }
                interfaceItKeyAction(ledNumber, direction);

                if (IsPolling)
                {
                    _polling.Push(new KeyValuePair<int, uint>(ledNumber, direction));
                }
            }
        }
        File.AppendAllText("Log.txt", ethernetData + Environment.NewLine);
    }

    private readonly Stack<KeyValuePair<int, uint>> _polling = new();

    public bool GetSwitch(out int ledNumber, out uint direction)
    {
        ledNumber = 0;
        direction = 0;
        if (_polling.Count == 0)
        {
            return false;
        }
        KeyValuePair<int, uint> keyValuePair = _polling.Pop();
        ledNumber = keyValuePair.Key;
        direction = keyValuePair.Value;
        return true;
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
            case "ID":
                InterfaceItEthernetInfo.Id = value;
                break;

            case "NAME":
                InterfaceItEthernetInfo.Name = value;
                break;

            case "SERIAL":
                InterfaceItEthernetInfo.SerialNumber = value;
                break;

            case "DESC":
                InterfaceItEthernetInfo.Description = value;
                break;

            case "COPYRIGHT":
                InterfaceItEthernetInfo.Copyright = value;
                break;

            case "VERSION":
                InterfaceItEthernetInfo.Version = value;
                break;

            case "FIRMWARE":
                InterfaceItEthernetInfo.Firmware = value;
                break;

            case "LOCATION":
                InterfaceItEthernetInfo.Location = Convert.ToByte(value);
                break;

            case "USAGE":
                InterfaceItEthernetInfo.Usage = Convert.ToByte(value);
                break;

            case "HOSTNAME":
                InterfaceItEthernetInfo.HostName = value;
                break;

            case "CLIENT":
                InterfaceItEthernetInfo.Client = value;
                break;

            case "BOARD":
                string[] board = value.Split(':');
                InterfaceItEthernetInfo.Boards ??= [];
                InterfaceItEthernetInfo.Boards.Add(new InterfaceItEthernetBoardInfo { BoardNumber = Convert.ToByte(board[0]), Id = board[1], Description = board[2] });
                break;

            case "CONFIG":
                GetConfigData(value);
                break;
        }
    }

    private void GetConfigData(string value)
    {
        string[] config = value.Split(":");
        int boardNumberMinusOne = Convert.ToInt32(config[0]) -1;
        switch (config[1])
        {
            case "LED":
                InterfaceItEthernetInfo.Boards[boardNumberMinusOne].LedsConfig = GetConfigData(config);
                break;

            case "SWITCH":
                InterfaceItEthernetInfo.Boards[boardNumberMinusOne].SwitchesConfig = GetConfigData(config);
                break;

            case "7 SEGMENT":
                InterfaceItEthernetInfo.Boards[boardNumberMinusOne].SevenSegmentsConfig = GetConfigData(config);
                break;

            case "DATALINE":
                InterfaceItEthernetInfo.Boards[boardNumberMinusOne].DataLinesConfig = GetConfigData(config);
                break;

            case "ENCODER":
                InterfaceItEthernetInfo.Boards[boardNumberMinusOne].EncodersConfig = GetConfigData(config);
                break;

            case "ANALOG IN":
                InterfaceItEthernetInfo.Boards[boardNumberMinusOne].AnalogInputsConfig = GetConfigData(config);
                break;

            case "PULSE WIDTH":
                InterfaceItEthernetInfo.Boards[boardNumberMinusOne].PulseWidthsConfig = GetConfigData(config);
                break;
        }
    }

    private static InterfaceItEthernetBoardConfig GetConfigData(string[] config)
    {
        return new InterfaceItEthernetBoardConfig
        {
            StartIndex = Convert.ToInt32(config[3]),
            StopIndex = Convert.ToInt32(config[5]),
            TotalCount = Convert.ToInt32(config[7])
        };
    }

    private void CloseStream()
    {
        try
        {
            SendinterfaceItEthernetLedAllOff();
            stream?.Write(Encoding.ASCII.GetBytes("DISCONNECT" + "\r\n"));
            client?.Close();
        }
        catch (Exception e)
        {
            // ignored
        }
    }

    private void SendinterfaceItEthernetLedAllOff()
    {
        for (int i = InterfaceItEthernetInfo.Boards[0].LedsConfig.StartIndex; i <= InterfaceItEthernetInfo.Boards[0].LedsConfig.TotalCount; i++)
        {
            stream?.Write(Encoding.ASCII.GetBytes("B1:LED:" + i + ":" + 0 + "\r\n"));
        }
        //stream?.Write(Encoding.ASCII.GetBytes("B1:CLEAR" + "\r\n"));
    }

    public void SendinterfaceItEthernetLed(int nLed, bool bOn)
    {
        SendinterfaceItEthernetLed<bool>(nLed, bOn);
    }

    public void SendinterfaceItEthernetLed(int nLed, int bOn)
    {
        SendinterfaceItEthernetLed<int>(nLed, bOn);
    }

    public void SendinterfaceItEthernetLed(int nLed, double bOn)
    {
        SendinterfaceItEthernetLed<double>(nLed, bOn);
    }

    bool _hasErrorBeenShown = hasErrorBeenShown;
    private void SendinterfaceItEthernetLed<T>(int nLed, T bOn)
    {
        if (nLed < 0)
        {
            throw new ArgumentException("nLED must be a non-negative integer.");
        }

        if (bOn is not bool && bOn is not int && bOn is not double)
        {
            throw new ArgumentException("bOn must be of type bool, int, or double.");
        }

        Reconnect();

        try
        {
            stream?.Write(Encoding.ASCII.GetBytes("B1:LED:" + nLed + ":" + Convert.ToUInt16(bOn) + "\r\n"));
        }
        catch (Exception e)
        {
            Reconnect();
            if (!_hasErrorBeenShown)
            {
                _hasErrorBeenShown = true;
            }
        }
    }

    private void Reconnect()
    {
        if (!_hasErrorBeenShown)
            return;
        
        try
        {
            client = new();
            client.Connect(HostIpAddress, TcpPort);
            stream = client.GetStream();
            _hasErrorBeenShown = false;

        }
        catch (OperationCanceledException)
        {

        }
        catch (ArgumentNullException)
        {

        }
        catch (SocketException)
        {
        }
    }
}

public readonly struct InterfaceItEthernetDiscovery
{
    public string HostName { get; init; }
    public string MacAddress { get; init; }
    public string IpAddress { get; init; }
    public string Message { get; init; }
    public string Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
}

public class InterfaceItEthernetInfo
{
    public string HostIpAddress { get; set; }
    public string Id { get; set; }
    public string Name { get; set; }
    public string SerialNumber { get; set; }
    public string Description { get; set; }
    public string Copyright { get; set; }
    public string Version { get; set; }
    public string Firmware { get; set; }
    public byte Location { get; set; }
    public byte Usage { get; set; }
    public string HostName { get; set; }
    public string Client { get; set; }
    public List<InterfaceItEthernetBoardInfo> Boards { get; set; }
}

public class InterfaceItEthernetBoardInfo
{
    public byte BoardNumber { get; set; }
    public string Id { get; set; }
    public string Description { get; set; }
    public InterfaceItEthernetBoardConfig LedsConfig { get; set; }
    public InterfaceItEthernetBoardConfig SwitchesConfig { get; set; }
    public InterfaceItEthernetBoardConfig SevenSegmentsConfig { get; set; }
    public InterfaceItEthernetBoardConfig DataLinesConfig { get; set; }
    public InterfaceItEthernetBoardConfig EncodersConfig { get; set; }
    public InterfaceItEthernetBoardConfig AnalogInputsConfig { get; set; }
    public InterfaceItEthernetBoardConfig PulseWidthsConfig { get; set; }
}

public class InterfaceItEthernetBoardConfig
{
    public int StartIndex { get; set; }
    public int StopIndex { get; set; }
    public int TotalCount { get; set; }
}