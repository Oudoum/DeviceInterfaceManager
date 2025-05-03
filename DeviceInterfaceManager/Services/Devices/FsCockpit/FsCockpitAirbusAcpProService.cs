using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.Services.Devices.FsCockpit;

public class FsCockpitAirbusAcpProService : FsCockpitAirbusAcpService
{
    public FsCockpitAirbusAcpProService(FsCockpitSerialPortService fsCockpitSerialPortService) : base(fsCockpitSerialPortService)
    {
        Outputs.Builder outputsBuilder = new();
        Outputs = outputsBuilder.SetLedInfo(0, 18).SetDatalineInfo(0, 14).SetAnalogInfo(5, 8).Build();
    }

    protected override void AdditionalStartupRequest()
    {
        SendCommand((byte)Commands.RequestPotentiometerSwitchPosition);
    }

    protected override void SetLed(byte byte1, byte byte2, byte byte3, byte byte4)
    {
        SendCommand((byte)Commands.SetKeyIndicators, byte1, byte2, byte3);
    }

    protected override void ClearLed(byte byte1, byte byte2, byte byte3, byte byte4)
    {
        SendCommand((byte)Commands.ClearKeyIndicators, byte1, byte2, byte3);
    }

    private enum Commands : byte
    {
        SetKeyIndicators = 0x09,
        ClearKeyIndicators = 0x0A,
        RequestPotentiometerSwitchPosition = 0x0F
    }
}