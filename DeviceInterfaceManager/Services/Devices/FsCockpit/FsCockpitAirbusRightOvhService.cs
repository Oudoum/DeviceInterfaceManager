using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.Services.Devices.FsCockpit;

public class FsCockpitAirbusRightOvhService : FsCockpitServiceBase
{
    public FsCockpitAirbusRightOvhService(FsCockpitSerialPortService fsCockpitSerialPortService) : base(fsCockpitSerialPortService)
    {
        Inputs.Builder inputsBuilder = new();
        Outputs.Builder outputsBuilder = new();
        Inputs = inputsBuilder.SetSwitchInfo(0, 19).SetAnalogInfo(64, 64).Build();
        Outputs = outputsBuilder.SetLedInfo(0, 27).SetAnalogInfo(5, 9).Build();
    }

    protected override void StartupRequest()
    {
        SendCommand((byte)Commands.RequestWiperSelectorPosition);
        SendCommand((byte)Commands.RequestAftPotentiometerValue);
    }

    protected override void SetLed(byte byte1, byte byte2, byte byte3, byte byte4)
    {
        SendCommand((byte)Commands.SetKeyIndicators, byte1, byte2, byte3, byte4);
    }

    protected override void ClearLed(byte byte1, byte byte2, byte byte3, byte byte4)
    {
        SendCommand((byte)Commands.ClearKeyIndicators, byte1, byte2, byte3, byte4);
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
        if ((Commands)position is
            Commands.SetBacklightBrightness or
            Commands.SetAmberIndicatorsBrightness or
            Commands.SetWhiteIndicatorsBrightness or
            Commands.SetRedIndicatorsBrightness or
            Commands.SetBlueIndicatorsBrightness)
        {
            SendCommand(position, value);
        }
    }

    protected override bool GetValue(byte responses)
    {
        switch ((Responses)responses)
        {
            case Responses.ElacTwoPressed:
            case Responses.SecTwoPressed:
            case Responses.SecThreePressed:
            case Responses.FacTwoPressed:
            case Responses.HotAirPressed:
            case Responses.AftIsolValvePressed:
            case Responses.DischFwdPressed:
            case Responses.DischAftPressed:
            case Responses.TestPressed:
            case Responses.BlowerPressed:
            case Responses.ExtractPressed:
            case Responses.CabFansPressed:
            case Responses.ManStartOnePressed:
            case Responses.ManStartTwoPressed:
            case Responses.N1ModeOnePressed:
            case Responses.N1ModeTwoPressed:
            case Responses.RainRplntPressed:
                OnSwitchPositionChanged(Buffer.Dequeue(), true);
                return true;

            case Responses.WiperOffSelected:
            case Responses.WiperSlowSelected:
            case Responses.WiperFastSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.WiperOffSelected, (byte)Responses.WiperFastSelected);

            case Responses.ElacTwoReleased:
            case Responses.SecTwoReleased:
            case Responses.SecThreeReleased:
            case Responses.FacTwoReleased:
            case Responses.HotAirReleased:
            case Responses.AftIsolValveReleased:
            case Responses.DischFwdReleased:
            case Responses.DischAftReleased:
            case Responses.TestReleased:
            case Responses.BlowerReleased:
            case Responses.ExtractReleased:
            case Responses.CabFansReleased:
            case Responses.ManStartOneReleased:
            case Responses.ManStartTwoReleased:
            case Responses.N1ModeOneReleased:
            case Responses.N1ModeTwoReleased:
            case Responses.RainRplntReleased:
                OnSwitchPositionChanged(Buffer.Dequeue() - (byte)Responses.ElacTwoReleased, false);
                return true;

            case Responses.AftPotentiometerValue when Buffer.Count >= 2:
                return ProcessAnalogValueNormal((byte)Responses.AftPotentiometerValue, GetFrame(2));

            case Responses.AftPotentiometerValue:
                return false;

            default:
                Buffer.TryDequeue(out byte _);
                return true;
        }
    }

    private enum Commands : byte
    {
        SetBacklightBrightness = 0x05,
        SetAmberIndicatorsBrightness = 0x06,
        SetWhiteIndicatorsBrightness = 0x07,
        SetRedIndicatorsBrightness = 0x08,
        SetBlueIndicatorsBrightness = 0x09,
        SetKeyIndicators = 0x0A,
        ClearKeyIndicators = 0x0B,
        RequestWiperSelectorPosition = 0x0C,
        RequestAftPotentiometerValue = 0x0D
    }

    private enum Responses : byte
    {
        ElacTwoPressed = 0x00,
        SecTwoPressed = 0x01,
        SecThreePressed = 0x02,
        FacTwoPressed = 0x03,
        HotAirPressed = 0x04,
        AftIsolValvePressed = 0x05,
        DischFwdPressed = 0x06,
        DischAftPressed = 0x07,
        TestPressed = 0x08,
        BlowerPressed = 0x09,
        ExtractPressed = 0x0A,
        CabFansPressed = 0x0B,
        ManStartOnePressed = 0x0C,
        ManStartTwoPressed = 0x0D,
        N1ModeOnePressed = 0x0E,
        N1ModeTwoPressed = 0x0F,
        RainRplntPressed = 0x10,
        WiperOffSelected = 0x11,
        WiperSlowSelected = 0x12,
        WiperFastSelected = 0x13,
        ElacTwoReleased = 0x20,
        SecTwoReleased = 0x21,
        SecThreeReleased = 0x22,
        FacTwoReleased = 0x23,
        HotAirReleased = 0x24,
        AftIsolValveReleased = 0x25,
        DischFwdReleased = 0x26,
        DischAftReleased = 0x27,
        TestReleased = 0x28,
        BlowerReleased = 0x29,
        ExtractReleased = 0x2A,
        CabFansReleased = 0x2B,
        ManStartOneReleased = 0x2C,
        ManStartTwoReleased = 0x2D,
        N1ModeOneReleased = 0x2E,
        N1ModeTwoReleased = 0x2F,
        RainRplntReleased = 0x30,
        AftPotentiometerValue = 0x40
    }
}