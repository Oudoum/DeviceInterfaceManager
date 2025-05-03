using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.Services.Devices.FsCockpit;

public class FsCockpitAirbusWrClpService : FsCockpitServiceBase
{
    public FsCockpitAirbusWrClpService(FsCockpitSerialPortService fsCockpitSerialPortService) : base(fsCockpitSerialPortService)
    {
        Inputs.Builder inputsBuilder = new();
        Outputs.Builder outputsBuilder = new();
        Inputs = inputsBuilder.SetSwitchInfo(0, 13).SetAnalogInfo(14, 17).Build();
        Outputs = outputsBuilder.SetAnalogInfo(5, 5).Build();
    }

    protected override void StartupRequest()
    {
        SendCommand((byte)Commands.RequestFloodLtPotentiometerSwitch);
        SendCommand((byte)Commands.RequestIntegLtPotentiometerSwitch);
        SendCommand((byte)Commands.RequestOneOffTwoSwitch);
        SendCommand((byte)Commands.RequestGainPotentiometerSwitch);
        SendCommand((byte)Commands.RequestImageModeSelectorPosition);
        SendCommand((byte)Commands.RequestWindShearSwitch);
        SendCommand((byte)Commands.RequestFloodLtPotentiometerValue);
        SendCommand((byte)Commands.RequestIntegLtPotentiometerValue);
        SendCommand((byte)Commands.RequestGainPotentiometerValue);
        SendCommand((byte)Commands.RequestTiltPotentiometerValue);
    }

    protected override void SetLed(byte byte1, byte byte2, byte byte3, byte byte4)
    {
    }

    protected override void ClearLed(byte byte1, byte byte2, byte byte3, byte byte4)
    {
    }

    protected override void SetDataline(byte byte1, byte byte2, byte byte3, byte byte4)
    {
    }

    protected override void ClearDataline(byte byte1, byte byte2, byte byte3, byte byte4)
    {
    }

    protected override void SetDisplay(byte position, string data)
    {
    }

    protected override void SetAnalog(byte position, byte value)
    {
        if ((Commands)position == Commands.SetBacklightBrightness)
        {
            SendCommand(position, value);
        }
    }

    protected override bool GetValue(byte responses)
    {
        switch ((Responses)responses)
        {
            case Responses.FloodLtOffSelected:
            case Responses.FloodLtOnSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.FloodLtOffSelected, (byte)Responses.FloodLtOnSelected);

            case Responses.IntegLtOffSelected:
            case Responses.IntegLtOnSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.IntegLtOffSelected, (byte)Responses.IntegLtOnSelected);

            case Responses.OneSelected:
            case Responses.OffSelected:
            case Responses.TwoSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.OneSelected, (byte)Responses.TwoSelected);

            case Responses.GainAutoSelected:
            case Responses.GainManualSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.GainAutoSelected, (byte)Responses.GainManualSelected);

            case Responses.WxSelected:
            case Responses.WxTurbSelected:
            case Responses.MapSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.WxSelected, (byte)Responses.MapSelected);

            case Responses.WindShearAutoSelected:
            case Responses.WindShearOffSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.WindShearAutoSelected, (byte)Responses.WindShearOffSelected);

            case Responses.FloodLtPotentiometerValue when Buffer.Count >= 2:
                return ProcessAnalogValueNormal((byte)Responses.FloodLtPotentiometerValue, GetFrame(2));

            case Responses.FloodLtPotentiometerValue:
                return false;

            case Responses.IntegLtPotentiometerValue when Buffer.Count >= 2:
                return ProcessAnalogValueNormal((byte)Responses.IntegLtPotentiometerValue, GetFrame(2));

            case Responses.IntegLtPotentiometerValue:
                return false;

            case Responses.GainPotentiometerValue when Buffer.Count >= 2:
                return ProcessAnalogValueNormal((byte)Responses.GainPotentiometerValue, GetFrame(2));

            case Responses.GainPotentiometerValue:
                return false;

            case Responses.TiltPotentiometerValue when Buffer.Count >= 2:
                return ProcessAnalogValueNormal((byte)Responses.TiltPotentiometerValue, GetFrame(2));

            case Responses.TiltPotentiometerValue:
                return false;

            default:
                Buffer.TryDequeue(out byte _);
                return true;
        }
    }

    private enum Commands : byte
    {
        SetBacklightBrightness = 0x05,
        RequestFloodLtPotentiometerSwitch = 0x06,
        RequestIntegLtPotentiometerSwitch = 0x07,
        RequestOneOffTwoSwitch = 0x08,
        RequestGainPotentiometerSwitch = 0x09,
        RequestImageModeSelectorPosition = 0x0A,
        RequestWindShearSwitch = 0x0B,
        RequestFloodLtPotentiometerValue = 0x0C,
        RequestIntegLtPotentiometerValue = 0x0D,
        RequestGainPotentiometerValue = 0x0E,
        RequestTiltPotentiometerValue = 0x0F
    }

    private enum Responses : byte
    {
        FloodLtOffSelected = 0x00,
        FloodLtOnSelected = 0x01,
        IntegLtOffSelected = 0x02,
        IntegLtOnSelected = 0x03,
        OneSelected = 0x04,
        OffSelected = 0x05,
        TwoSelected = 0x06,
        GainAutoSelected = 0x07,
        GainManualSelected = 0x08,
        WxSelected = 0x09,
        WxTurbSelected = 0x0A,
        MapSelected = 0x0B,
        WindShearAutoSelected = 0x0C,
        WindShearOffSelected = 0x0D,
        FloodLtPotentiometerValue = 0x0E,
        IntegLtPotentiometerValue = 0x0F,
        GainPotentiometerValue = 0x10,
        TiltPotentiometerValue = 0x11
    }
}