using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia;
using Avalonia.Controls;
using DeviceInterfaceManager.Models.Devices;
using Microsoft.Extensions.Logging;

namespace DeviceInterfaceManager.Services.Devices.Poldragonet;

public class PoldragonetEthernetService : DeviceServiceBase
{
    private readonly ILogger _logger;
    private readonly UdpClient _client;
    private readonly IPEndPoint _endpoint;

    private ushort _commandCounter;

    public PoldragonetEthernetService(string? iPAddress, ILogger logger)
    {
        _logger = logger;

        if (string.IsNullOrEmpty(iPAddress))
        {
            iPAddress = "127.0.0.1";
        }

        Id = iPAddress;
        DeviceName = "Poldragonet A320 Throttle Quadrant";
        Icon = (Geometry?)Application.Current!.FindResource("Ethernet");

        Inputs.Builder inputsBuilder = new();
        Outputs.Builder outputsBuilder = new();
        Inputs = inputsBuilder.SetSwitchInfo(1, 6).SetAnalogInfo(1, 3).Build();
        Outputs = outputsBuilder.SetLedInfo(1, 4).SetAnalogInfo(1, 2).Build();

        _endpoint = new IPEndPoint(IPAddress.Parse(iPAddress), Protocol.Port);
        _client = new UdpClient(new IPEndPoint(IPAddress.Any, Protocol.Port));
    }

    private int _ledState;

    public override async Task SetLedAsync(int position, bool isEnabled)
    {
        int bit = 1 << (position - 1);
        _ledState = isEnabled ? _ledState | bit : _ledState & ~bit;
        await SetLedOutput((byte)_ledState);
    }

    public override Task SetDatalineAsync(int position, bool isEnabled)
    {
        throw new NotImplementedException();
    }

    public override Task SetSevenSegmentAsync(int position, string data)
    {
        throw new NotImplementedException();
    }

    public override async Task SetAnalogAsync(int position, double value)
    {
        byte? valueInByte = null;
        try
        {
            valueInByte = Convert.ToByte(value);
        }
        catch (Exception)
        {
            //
        }

        if (valueInByte is null)
        {
            return;
        }

        switch (position)
        {
            case 1:
                await SetTrim(valueInByte.Value);
                break;

            case 2:
                await SetBacklight(valueInByte.Value);
                break;
        }
    }

    public override async Task ResetAllOutputsAsync()
    {
        await SetLedOutput(0);
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
        await Send(Protocol.CmdStart);

        _ = Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    UdpReceiveResult result = await _client.ReceiveAsync(cancellationToken);
                    Parse(result.Buffer);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error with socket connection.");
                }
            }
        }, cancellationToken);

        return true;
    }

    private bool _disc1;
    private bool _disc2;

    private bool _engMaster2;
    private bool _engMaster1;

    private bool _engModeCrank;
    private bool _engModeStart;

    private void UpdateSwitchBit(ref bool field, byte value, byte mask, int position)
    {
        bool newValue = (value & mask) != 0;

        if (field == newValue)
        {
            return;
        }

        field = newValue;
        OnSwitchPositionChanged(position, newValue);
    }

    private void Parse(byte[] frame)
    {
        if (!frame.StartsWith(Protocol.Prefix))
        {
            return;
        }

        Span<byte> span = frame;
        span = span[(Protocol.Prefix.Length + Protocol.FrameCounterSize)..];

        if (span.StartsWith(Protocol.CmdPrefix))
        {
            _ = span[Protocol.CmdPrefix.Length..];
        }

        else if (span.StartsWith(Protocol.EventPrefix))
        {
            span = span[Protocol.EventPrefix.Length..];

            if (span.StartsWith(Protocol.Heartbeat))
            {
                return;
            }

            if (span.StartsWith(Protocol.Buttons) || span.StartsWith(Protocol.AutoResponse))
            {
                span = span[Protocol.Buttons.Length..];
                byte value = span[0];

                UpdateSwitchBit(ref _disc1, value, 0x01, 1);
                UpdateSwitchBit(ref _disc2, value, 0x02, 2);
                UpdateSwitchBit(ref _engMaster2, value, 0x10, 3);
                UpdateSwitchBit(ref _engMaster1, value, 0x20, 4);
                UpdateSwitchBit(ref _engModeCrank, value, 0x40, 5);
                UpdateSwitchBit(ref _engModeStart, value, 0x80, 6);
            }

            if (!span.StartsWith(Protocol.Axis))
            {
                return;
            }

            span = span[Protocol.Axis.Length..];

            short axisValue = BitConverter.ToInt16(span[1..3]);

            switch (span[0])
            {
                case 0x60:
                    OnAnalogInValueChanged(1, axisValue);
                    return;

                case 0x61:
                    OnAnalogInValueChanged(2, axisValue);
                    return;

                case 0x62:
                    OnAnalogInValueChanged(3, axisValue);
                    return;

                default:
                    return;
            }
        }
    }

    private byte[] BuildFrame(byte[] command)
    {
        // fullCommand = prefix + counter(2) + cmdPrefix + command
        byte[] fullCommand = new byte[
            Protocol.Prefix.Length +
            Protocol.FrameCounterSize +
            Protocol.CmdPrefix.Length +
            command.Length
        ];

        int offset = 0;

        // Prefix
        Buffer.BlockCopy(Protocol.Prefix, 0, fullCommand, offset, Protocol.Prefix.Length);
        offset += Protocol.Prefix.Length;

        // Counter (big endian)
        fullCommand[offset++] = (byte)(_commandCounter & 0xFF);
        fullCommand[offset++] = (byte)((_commandCounter >> 8) & 0xFF);

        // Command prefix (always 01 00)
        Buffer.BlockCopy(Protocol.CmdPrefix, 0, fullCommand, offset, Protocol.CmdPrefix.Length);
        offset += Protocol.CmdPrefix.Length;

        // Actual command payload
        Buffer.BlockCopy(command, 0, fullCommand, offset, command.Length);

        _commandCounter++;
        return fullCommand;
    }

    private async Task Send(byte[] command)
    {
        byte[] frame = BuildFrame(command);
        try
        {
            await _client.SendAsync(frame, frame.Length, _endpoint);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error with socket connection.");
        }
    }

    private async Task SetLedOutput(byte outputByte)
    {
        byte[] cmd = new byte[Protocol.CmdOutputBase.Length + 3];
        Buffer.BlockCopy(Protocol.CmdOutputBase, 0, cmd, 0, 3);

        cmd[3] = 0xF4;
        cmd[4] = 0x10;
        cmd[5] = outputByte;

        await Send(cmd);
    }

    private async Task SetTrim(byte value)
    {
        byte[] cmd = new byte[Protocol.CmdOutputBase.Length + 3];
        Buffer.BlockCopy(Protocol.CmdOutputBase, 0, cmd, 0, 3);

        cmd[3] = 0xF4;
        cmd[4] = 0x52;
        cmd[5] = value;

        await Send(cmd);
    }

    private async Task SetBacklight(byte brightness)
    {
        byte[] cmd = new byte[Protocol.CmdBacklightBase.Length + 2];
        Buffer.BlockCopy(Protocol.CmdBacklightBase, 0, cmd, 0, 3);

        cmd[3] = 0x70;
        cmd[4] = brightness;

        await Send(cmd);
    }

    public override async Task Disconnect()
    {
        await Send(Protocol.CmdStop);

        try
        {
            _client.Close();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error with socket connection.");
        }
    }

    private static class Protocol
    {
        public static readonly byte[] Prefix = [.. "A320__"u8];
        public const byte FrameCounterSize = 2;
        public const int Port = 45400;

        // Commands
        public static readonly byte[] CmdPrefix = [0x01, 0x00];
        public static readonly byte[] CmdStart = [0x12, 0x01, 0x00, 0x00];
        public static readonly byte[] CmdStop = [0x12, 0x01, 0x00, 0x01];

        public static readonly byte[] CmdFirmwareController = [.. "F\0\0"u8];
        public static readonly byte[] CmdFirmwareThrottle = [0x52, 0x03, 0x00, 0x93, 0xF4, 0x46];

        // Output commands
        public static readonly byte[] CmdOutputBase = [0x51, 0x03, 0x00];

        // Backlight
        public static readonly byte[] CmdBacklightBase = [0x51, 0x02, 0x00];

        //Event
        public static readonly byte[] EventPrefix = [.. "\0\0"u8];

        public static readonly byte[] Heartbeat = [0xA0, 0x01, 0x00, 0xF4];
        public static readonly byte[] Buttons = [0xA0, 0x03, 0x00, 0xF4, 0x00];
        public static readonly byte[] Axis = [0xA0, 0x04, 0x00, 0xF4];
        public static readonly byte[] AutoResponse = [0xA0, 0x15, 0x00, 0xF4, 0x00];
    }
}