using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.Services.Devices.FsCockpit;

public class FsCockpitAirbusLeftOvhService : FsCockpitServiceBase
{
    public FsCockpitAirbusLeftOvhService(FsCockpitSerialPortService fsCockpitSerialPortService) : base(fsCockpitSerialPortService)
    {
        Inputs.Builder inputsBuilder = new();
        Outputs.Builder outputsBuilder = new();
        Inputs = inputsBuilder.SetSwitchInfo(0, 26).Build();
        Outputs = outputsBuilder.SetLedInfo(0, 17).SetAnalogInfo(5, 9).Build();
    }

    protected override void StartupRequest()
    {
        SendCommand((byte)Commands.RequestWiperSelectorPosition);
        SendCommand((byte)Commands.RequestCaptPursSwitchPosition);
    }

    protected override void SetLed(byte byte1, byte byte2, byte byte3, byte byte4)
    {
        SendCommand((byte)Commands.SetKeyIndicators, byte1, byte2, byte3);
    }

    protected override void ClearLed(byte byte1, byte byte2, byte byte3, byte byte4)
    {
        SendCommand((byte)Commands.ClearKeyIndicators, byte1, byte2, byte3);
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
            case Responses.CommandPressed:
            case Responses.HornShutOffPressed:
            case Responses.EmerGenTestPressed:
            case Responses.GenOneLinePressed:
            case Responses.ManOnPressed:
            case Responses.TerrPressed:
            case Responses.SysPressed:
            case Responses.GsModePressed:
            case Responses.FlapModePressed:
            case Responses.LdgFlapThreePressed:
            case Responses.GndCtlPressed:
            case Responses.CvrErasePressed:
            case Responses.CvrTestPressed:
            case Responses.MaskManOnPressed:
            case Responses.PassengerPressed:
            case Responses.CrewSupplyPressed:
            case Responses.MechPressed:
            case Responses.AllPressed:
            case Responses.FwdPressed:
            case Responses.AfrPressed:
            case Responses.EmerPressed:
            case Responses.RainRplntPressed:
                OnSwitchPositionChanged(Buffer.Dequeue(), true);
                return true;

            case Responses.CaptPursSelected:
            case Responses.CaptSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.CaptPursSelected, (byte)Responses.CaptSelected);

            case Responses.WiperOffSelected:
            case Responses.WiperSlowSelected:
            case Responses.WiperFastSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.WiperOffSelected, (byte)Responses.WiperFastSelected);

            case Responses.CommandReleased:
            case Responses.HornShutOffReleased:
            case Responses.EmerGenTestReleased:
            case Responses.GenOneLineReleased:
            case Responses.ManOnReleased:
            case Responses.TerrReleased:
            case Responses.SysReleased:
            case Responses.GsModeReleased:
            case Responses.FlapModeReleased:
            case Responses.LdgFlapThreeReleased:
            case Responses.GndCtlReleased:
            case Responses.CvrEraseReleased:
            case Responses.CvrTestReleased:
            case Responses.MaskManOnReleased:
            case Responses.PassengerReleased:
            case Responses.CrewSupplyReleased:
            case Responses.MechReleased:
            case Responses.AllReleased:
            case Responses.FwdReleased:
            case Responses.AfrReleased:
            case Responses.EmerReleased:
            case Responses.RainRplntReleased:
                OnSwitchPositionChanged(Buffer.Dequeue() - (byte)Responses.CommandReleased, false);
                return true;

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
        RequestCaptPursSwitchPosition = 0x0D
    }

    private enum Responses : byte
    {
        CommandPressed = 0x00,
        HornShutOffPressed = 0x01,
        EmerGenTestPressed = 0x02,
        GenOneLinePressed = 0x03,
        ManOnPressed = 0x04,
        TerrPressed = 0x05,
        SysPressed = 0x06,
        GsModePressed = 0x07,
        FlapModePressed = 0x08,
        LdgFlapThreePressed = 0x09,
        GndCtlPressed = 0x0A,
        CvrErasePressed = 0x0B,
        CvrTestPressed = 0x0C,
        MaskManOnPressed = 0x0D,
        PassengerPressed = 0x0E,
        CrewSupplyPressed = 0x0F,
        MechPressed = 0x10,
        AllPressed = 0x11,
        FwdPressed = 0x12,
        AfrPressed = 0x13,
        EmerPressed = 0x14,
        RainRplntPressed = 0x15,
        CaptPursSelected = 0x16,
        CaptSelected = 0x17,
        WiperOffSelected = 0x18,
        WiperSlowSelected = 0x19,
        WiperFastSelected = 0x1A,
        CommandReleased = 0x20,
        HornShutOffReleased = 0x21,
        EmerGenTestReleased = 0x22,
        GenOneLineReleased = 0x23,
        ManOnReleased = 0x24,
        TerrReleased = 0x25,
        SysReleased = 0x26,
        GsModeReleased = 0x27,
        FlapModeReleased = 0x28,
        LdgFlapThreeReleased = 0x29,
        GndCtlReleased = 0x2A,
        CvrEraseReleased = 0x2B,
        CvrTestReleased = 0x2C,
        MaskManOnReleased = 0x2D,
        PassengerReleased = 0x2E,
        CrewSupplyReleased = 0x2F,
        MechReleased = 0x30,
        AllReleased = 0x31,
        FwdReleased = 0x32,
        AfrReleased = 0x33,
        EmerReleased = 0x34,
        RainRplntReleased = 0x35
    }
}