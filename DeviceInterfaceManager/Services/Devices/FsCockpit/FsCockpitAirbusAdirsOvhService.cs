using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.Services.Devices.FsCockpit;

public class FsCockpitAirbusAdirsOvhService : FsCockpitServiceBase
{
    public FsCockpitAirbusAdirsOvhService(FsCockpitSerialPortService fsCockpitSerialPortService) : base(fsCockpitSerialPortService)
    {
        Inputs.Builder inputsBuilder = new();
        Outputs.Builder outputsBuilder = new();
        Inputs = inputsBuilder.SetSwitchInfo(0, 39).Build();
        Outputs = outputsBuilder.SetLedInfo(0, 13).SetDatalineInfo(0, 6).SetSevenSegmentInfo(10, 10).SetAnalogInfo(5, 9).Build();
    }

    protected override void StartupRequest()
    {
        SendCommand((byte)Commands.RequestDataSelectorPosition);
        SendCommand((byte)Commands.RequestSysSelectorPosition);
        SendCommand((byte)Commands.RequestIr1SelectorPosition);
        SendCommand((byte)Commands.RequestIr2SelectorPosition);
        SendCommand((byte)Commands.RequestIr3SelectorPosition);
        SendCommand((byte)Commands.RequestSpareSelectorPosition);
    }

    protected override void SetLed(byte byte1, byte byte2, byte byte3, byte byte4)
    {
        SendCommand((byte)Commands.SetKeyIndicators, byte1, byte2);
    }

    protected override void ClearLed(byte byte1, byte byte2, byte byte3, byte byte4)
    {
        SendCommand((byte)Commands.ClearKeyIndicators, byte1, byte2);
    }

    protected override void SetDataline(byte byte1, byte byte2, byte byte3, byte byte4)
    {
        SendCommand((byte)Commands.SetIndicators, byte1);
    }

    protected override void ClearDataline(byte byte1, byte byte2, byte byte3, byte byte4)
    {
        SendCommand((byte)Commands.ClearIndicators, byte1);
    }

    protected override void SetDisplay(byte position, string data)
    {
        byte[] dataArray = GetDisplayData(null, data, out byte _);

        if ((Commands)position == Commands.SetDisplay)
        {
            SendCommand(position,
                dataArray[0], dataArray[1], dataArray[2], dataArray[3],
                dataArray[4], dataArray[5], dataArray[6], dataArray[7],
                dataArray[8], dataArray[9], dataArray[10], dataArray[11],
                dataArray[12], dataArray[13], dataArray[14], dataArray[15]);
        }
    }

    protected override void SetAnalog(byte position, byte value)
    {
        if ((Commands)position is
            Commands.SetBacklightBrightness or
            Commands.SetDisplayBrightness or
            Commands.SetAmberIndicatorsBrightness or
            Commands.SetWhiteIndicatorsBrightness or
            Commands.SetGreenIndicatorsBrightness)
        {
            SendCommand(position, value);
        }
    }

    protected override bool GetValue(byte responses)
    {
        switch ((Responses)responses)
        {
            case Responses.ZeroPressed:
            case Responses.OnePressed:
            case Responses.TwoPressed:
            case Responses.ThreePressed:
            case Responses.FourPressed:
            case Responses.FivePressed:
            case Responses.SixPressed:
            case Responses.SevenPressed:
            case Responses.EightPressed:
            case Responses.NinePressed:
            case Responses.EntPressed:
            case Responses.ClrPressed:
            case Responses.Adr1Pressed:
            case Responses.Adr2Pressed:
            case Responses.Adr3Pressed:
            case Responses.Elac1Pressed:
            case Responses.Sec1Pressed:
            case Responses.Fac1Pressed:
                OnSwitchPositionChanged(Buffer.Dequeue(), true);
                return true;

            case Responses.DataWindSelected:
            case Responses.DataPPosSelected:
            case Responses.DataHdgSelected:
            case Responses.DataStsSelected:
            case Responses.DataTkGsSelected:
            case Responses.DataTestSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.DataWindSelected, (byte)Responses.DataTestSelected);

            case Responses.SysOffSelected:
            case Responses.Sys1Selected:
            case Responses.Sys3Selected:
            case Responses.Sys2Selected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.SysOffSelected, (byte)Responses.Sys2Selected);

            case Responses.Ir1OffSelected:
            case Responses.Ir1NavSelected:
            case Responses.Ir1AttSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.Ir1OffSelected, (byte)Responses.Ir1AttSelected);

            case Responses.Ir2OffSelected:
            case Responses.Ir2NavSelected:
            case Responses.Ir2AttSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.Ir2OffSelected, (byte)Responses.Ir2AttSelected);

            case Responses.Ir3OffSelected:
            case Responses.Ir3NavSelected:
            case Responses.Ir3AttSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.Ir3OffSelected, (byte)Responses.Ir3AttSelected);

            case Responses.SparePos1Selected:
            case Responses.SparePosNeutralSelected:
            case Responses.SparePos2Selected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.SparePos1Selected, (byte)Responses.SparePos2Selected);

            case Responses.ZeroReleased:
            case Responses.OneReleased:
            case Responses.TwoReleased:
            case Responses.ThreeReleased:
            case Responses.FourReleased:
            case Responses.FiveReleased:
            case Responses.SixReleased:
            case Responses.SevenReleased:
            case Responses.EightReleased:
            case Responses.NineReleased:
            case Responses.EntReleased:
            case Responses.ClrReleased:
            case Responses.Adr1Released:
            case Responses.Adr2Released:
            case Responses.Adr3Released:
            case Responses.Elac1Released:
            case Responses.Sec1Released:
            case Responses.Fac1Released:
                OnSwitchPositionChanged(Buffer.Dequeue() - (byte)Responses.ZeroReleased, false);
                return true;

            default:
                Buffer.TryDequeue(out byte _);
                return true;
        }
    }

    private enum Commands : byte
    {
        SetBacklightBrightness = 0x05,
        SetDisplayBrightness = 0x06,
        SetAmberIndicatorsBrightness = 0x07,
        SetWhiteIndicatorsBrightness = 0x08,
        SetGreenIndicatorsBrightness = 0x09,
        SetDisplay = 0x0A,
        SetIndicators = 0x0B,
        ClearIndicators = 0x0C,
        SetKeyIndicators = 0x0D,
        ClearKeyIndicators = 0x0E,
        RequestDataSelectorPosition = 0x0F,
        RequestSysSelectorPosition = 0x10,
        RequestIr1SelectorPosition = 0x11,
        RequestIr2SelectorPosition = 0x12,
        RequestIr3SelectorPosition = 0x13,
        RequestSpareSelectorPosition = 0x14
    }

    private enum Responses : byte
    {
        ZeroPressed = 0x00,
        OnePressed = 0x01,
        TwoPressed = 0x02,
        ThreePressed = 0x03,
        FourPressed = 0x04,
        FivePressed = 0x05,
        SixPressed = 0x06,
        SevenPressed = 0x07,
        EightPressed = 0x08,
        NinePressed = 0x09,
        EntPressed = 0x0A,
        ClrPressed = 0x0B,
        Adr1Pressed = 0x0C,
        Adr2Pressed = 0x0D,
        Adr3Pressed = 0x0E,
        Elac1Pressed = 0x0F,
        Sec1Pressed = 0x10,
        Fac1Pressed = 0x11,
        DataWindSelected = 0x12,
        DataPPosSelected = 0x13,
        DataHdgSelected = 0x14,
        DataStsSelected = 0x15,
        DataTkGsSelected = 0x16,
        DataTestSelected = 0x17,
        SysOffSelected = 0x18,
        Sys1Selected = 0x19,
        Sys3Selected = 0x1A,
        Sys2Selected = 0x1B,
        Ir1OffSelected = 0x1C,
        Ir1NavSelected = 0x1D,
        Ir1AttSelected = 0x1E,
        Ir2OffSelected = 0x1F,
        Ir2NavSelected = 0x20,
        Ir2AttSelected = 0x21,
        Ir3OffSelected = 0x22,
        Ir3NavSelected = 0x23,
        Ir3AttSelected = 0x24,
        SparePos1Selected = 0x25,
        SparePosNeutralSelected = 0x26,
        SparePos2Selected = 0x27,
        ZeroReleased = 0x40,
        OneReleased = 0x41,
        TwoReleased = 0x42,
        ThreeReleased = 0x43,
        FourReleased = 0x44,
        FiveReleased = 0x45,
        SixReleased = 0x46,
        SevenReleased = 0x47,
        EightReleased = 0x48,
        NineReleased = 0x49,
        EntReleased = 0x4A,
        ClrReleased = 0x4B,
        Adr1Released = 0x4C,
        Adr2Released = 0x4D,
        Adr3Released = 0x4E,
        Elac1Released = 0x4F,
        Sec1Released = 0x50,
        Fac1Released = 0x51
    }
}