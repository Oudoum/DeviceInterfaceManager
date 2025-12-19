using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceInterfaceManager.Services.Devices.FsCockpit;

public class FsCockpitSerialPortService : SerialPortService
{
    public readonly Queue<byte> Buffer = [];
    public Predicate<byte>? GetResponse;
    public string PanelModelPrefix { get; private set; } = string.Empty;
    public string SerialNumber { get; private set; } = string.Empty;
    public bool HasHighTensionDetents { get; private set; }

    public FsCockpitSerialPortService(string portName, bool hasHighTensionDetents) : base(portName, 115200, true)
    {
        HasHighTensionDetents = hasHighTensionDetents;
    }

    public async Task<IDeviceService?> GetDeviceServiceAsync(CancellationToken cancellationToken)
    {
        if (!Connect())
        {
            return null;
        }

        SendData((byte)DeviceCommands.ResetCommunicationStateMachine, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
        SendData((byte)DeviceCommands.GetDeviceSerialNumber);
        SendData((byte)DeviceCommands.GetDeviceMode);
        SendData((byte)DeviceCommands.GetFirmwareVersion);

        CancellationTokenSource cts = new();
        Task delayTask = Task.Delay(TimeSpan.FromSeconds(10), cts.Token);

        while (string.IsNullOrEmpty(PanelModelPrefix) || string.IsNullOrEmpty(SerialNumber))
        {
            if (delayTask.IsCompleted)
            {
                return null;
            }

            await Task.Yield();
        }

        await cts.CancelAsync();

        IDeviceService? deviceService = PanelModelPrefix switch
        {
            FsCockpitServiceBase.AirbusRmp => new FsCockpitAirbusRmpService(this),
            FsCockpitServiceBase.AirbusFcuLite => new FsCockpitAirbusFcuLiteService(this),
            FsCockpitServiceBase.AirbusEfisCpt => new FsCockpitAirbusEfisService(this),
            FsCockpitServiceBase.AirbusEfisFo => new FsCockpitAirbusEfisService(this),
            FsCockpitServiceBase.AirbusMcduLite => new FsCockpitAirbusMcduLiteService(this),
            FsCockpitServiceBase.AirbusEcamSw => new FsCockpitAirbusEcamSwService(this),
            FsCockpitServiceBase.AirbusTcasLpfo => new FsCockpitAirbusTcasLpfoService(this),
            FsCockpitServiceBase.AirbusTpbp => new FsCockpitAirbusTpbpService(this),
            FsCockpitServiceBase.AirbusAcp => new FsCockpitAirbusAcpService(this),
            FsCockpitServiceBase.AirbusWrClp => new FsCockpitAirbusWrClpService(this),
            FsCockpitServiceBase.AirbusMiniOvh => new FsCockpitAirbusMiniOvhService(this),
            FsCockpitServiceBase.AirbusThrottleEsp => new FsCockpitAirbusThrottleService(this),
            FsCockpitServiceBase.AirbusLeftOvh => new FsCockpitAirbusLeftOvhService(this),
            FsCockpitServiceBase.AirbusRightOvh => new FsCockpitAirbusRightOvhService(this),
            FsCockpitServiceBase.AirbusAdirsOvh => new FsCockpitAirbusAdirsOvhService(this),
            FsCockpitServiceBase.AirbusAirconOvh => new FsCockpitAirbusAirconOvhService(this),
            FsCockpitServiceBase.AirbusElecOvh => new FsCockpitAirbusElecOvhService(this),
            FsCockpitServiceBase.AirbusFuelHydFire => new FsCockpitAirbusFuelHydFireService(this),
            FsCockpitServiceBase.AirbusAbLgsPtiClk => new FsCockpitAirbusAbLgsPtiClkService(this),
            FsCockpitServiceBase.AirbusN2PfdCpt => new FsCockpitAirbusN2PfdService(this),
            FsCockpitServiceBase.AirbusN2PfdFo => new FsCockpitAirbusN2PfdService(this),
            FsCockpitServiceBase.AirbusAcpPro => new FsCockpitAirbusAcpProService(this),
            FsCockpitServiceBase.AirbusSidestickCpt => new FsCockpitAirbusSidestick(this),
            FsCockpitServiceBase.AirbusSidestickFo => new FsCockpitAirbusSidestick(this),
            _ => null
        };

        deviceService?.ConnectAsync(cancellationToken);
        return deviceService;
    }

    protected override void DataReceived(byte[] data)
    {
        foreach (byte b in data)
        {
            Buffer.Enqueue(b);
        }

        while (Buffer.Count > 0)
        {
            byte response = Buffer.Peek();
            switch ((DeviceResponses)response)
            {
                case DeviceResponses.DeviceSerialNumber when Buffer.Count >= 9:
                    byte[] serialNumber = GetFrame(9);
                    PanelModelPrefix = GetFrameByLength(serialNumber, 4);
                    SerialNumber = GetFrameByLength(serialNumber, 8);
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
                    if (GetResponse is not null && !GetResponse(response))
                    {
                        return;
                    }

                    break;
            }
        }
    }

    public byte[] GetFrame(int position)
    {
        byte[] frame = new byte[position];
        for (int i = 0; i < position; i++)
        {
            frame[i] = Buffer.Dequeue();
        }

        return frame;
    }

    private static string GetFrameByLength(byte[] frame, int length)
    {
        byte[] data = new byte[length];
        Array.Copy(frame, 1, data, 0, length);
        return Encoding.ASCII.GetString(data);
    }

    private enum DeviceCommands
    {
        GetDeviceSerialNumber = 0x00,
        GetDeviceMode = 0x01,
        GetFirmwareVersion = 0x02,
        ResetCommunicationStateMachine = 0xFF
    }

    private enum DeviceResponses
    {
        DeviceSerialNumber = 0x80,
        DeviceMode = 0x81,
        FirmwareVersion = 0x82
    }
}