using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceInterfaceManager.Services.Devices;

public abstract class FsCockpitServiceBase : DeviceSerialServiceBase
{
    protected readonly Queue<byte> Buffer = [];

    protected FsCockpitServiceBase() : base("COM3", 115200, true)
    {
    }

    public override Task<ConnectionStatus> ConnectAsync(CancellationToken cancellationToken)
    {
        ConnectionStatus status = base.ConnectAsync(cancellationToken).Result;
        if (status != ConnectionStatus.Connected)
        {
            return Task.FromResult(status);
        }

        SendData((byte)DeviceCommands.GetDeviceSerialNumber);
        return Task.FromResult(InitialRequest());
    }

    protected abstract ConnectionStatus InitialRequest();

    public override Task SetLedAsync(int position, bool isEnabled)
    {
        position = 1 << (position - 1);
        if (isEnabled)
        {
            SetIndicator((byte)position);
            return Task.CompletedTask;
        }

        ClearIndicator((byte)position);
        return Task.CompletedTask;
    }

    protected abstract void SetIndicator(byte position);

    protected abstract void ClearIndicator(byte position);

    public override Task SetDatalineAsync(int position, bool isEnabled)
    {
        SetDataline(position, isEnabled);
        return Task.CompletedTask;
    }

    protected abstract void SetDataline(int position, bool isEnabled);

    public override Task SetSevenSegmentAsync(int position, string data)
    {
        // Add later
        return Task.CompletedTask;
    }

    public override Task SetAnalogAsync(int position, double value)
    {
        byte byteValue;
        try
        {
            byteValue = Convert.ToByte(value);
        }
        catch (Exception)
        {
            return Task.CompletedTask;
        }

        SetAnalog((byte)position, byteValue);
        return Task.CompletedTask;
    }

    protected abstract void SetAnalog(byte position, byte value);

    protected override void DataReceived(byte[] data)
    {
        foreach (byte b in data)
        {
            Buffer.Enqueue(b);
        }

        while (Buffer.Count > 0)
        {
            byte responses = Buffer.Peek();
            switch ((DeviceResponses)responses)
            {
                case DeviceResponses.DeviceSerialNumber when Buffer.Count >= 9:
                    SetDevice(GetFrame(9));
                    break;

                case DeviceResponses.DeviceSerialNumber:
                    return;

                case DeviceResponses.DeviceMode when Buffer.Count >= 2:
                    GetFrame(2);
                    break;

                case DeviceResponses.DeviceMode:
                    return;

                case DeviceResponses.FirmwareVersion when Buffer.Count >= 5:
                    GetFrame(5);
                    break;

                case DeviceResponses.FirmwareVersion:
                    return;
                
                default:
                    if (!GetValue(responses))
                    {
                        return;
                    }
                    break;
            }
        }
    }

    protected abstract bool GetValue(byte responses);

    protected byte[] GetFrame(int position)
    {
        byte[] frame = new byte[position];
        for (int i = 0; i < position; i++)
        {
            frame[i] = Buffer.Dequeue();
        }

        return frame;
    }

    private void SetDevice(byte[] frame)
    {
        byte[] data = new byte[4];
        Array.Copy(frame, 1, data, 0, 4);
        SerialNumber.TryGetValue(Encoding.ASCII.GetString(data), out string? deviceName);
        DeviceName = deviceName;

        Array.Copy(frame, 5, data, 0, 4);
        Id = Encoding.ASCII.GetString(data);
    }

    public static async Task<IDeviceService?> GetFsCockpitService(CancellationToken cancellationToken)
    {
        FsCockpitAirbusThrottleServiceBase airbusThrottleService = new();
        return ConnectionStatus.Connected == await airbusThrottleService.ConnectAsync(cancellationToken) ? airbusThrottleService : null;
    }

    protected static readonly Dictionary<string, string?> SerialNumber = new()
    {
        { "AB05", "Airbus FCU Lite" },
        { "AB15", "Airbus Throttle/ESP" }
    };

    private enum DeviceCommands
    {
        GetDeviceSerialNumber = 0x00,
        GetDeviceMode = 0x01,
        GetFirmwareVersion = 0x02,
    }

    private enum DeviceResponses
    {
        DeviceSerialNumber = 0x80,
        DeviceMode = 0x81,
        FirmwareVersion = 0x82
    }
}