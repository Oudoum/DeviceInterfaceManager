using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.Services.Devices.FsCockpit;

public class FsCockpitAirbusEfisService : FsCockpitServiceBase
{
    private bool _skipNextEncoderFrame;

    public FsCockpitAirbusEfisService(FsCockpitSerialPortService fsCockpitSerialPortService) : base(fsCockpitSerialPortService)
    {
        Inputs.Builder inputsBuilder = new();
        Outputs.Builder outputsBuilder = new();
        Inputs = inputsBuilder.SetSwitchInfo(0, 32).Build();
        Outputs = outputsBuilder.SetLedInfo(0, 6).SetDatalineInfo(0, 6).SetSevenSegmentInfo(15, 15).SetAnalogInfo(5, 14).Build();
    }

    protected override void StartupRequest()
    {
        SendCommand((byte)Commands.RequestBaroSelectorPosition);
        SendCommand((byte)Commands.RequestModeSelectorPosition);
        SendCommand((byte)Commands.RequestRangeSelectorPosition);
        SendCommand((byte)Commands.RequestAdfVor1SelectorPosition);
        SendCommand((byte)Commands.RequestAdfVor2SelectorPosition);
    }

    protected override void SetLed(byte byte1, byte byte2, byte byte3, byte byte4)
    {
        SendCommand((byte)Commands.SetKeyIndicators, byte1);
    }

    protected override void ClearLed(byte byte1, byte byte2, byte byte3, byte byte4)
    {
        SendCommand((byte)Commands.ClearKeyIndicators, byte1);
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
        byte[] dataArray = GetDisplayData(FsCockpitAirbusFcuLiteService.DisplayDigitCodes, data, out byte decimalDigit);

        if ((Commands)position == Commands.SetDisplay)
        {
            SendCommand(position, dataArray[0], dataArray[1], dataArray[2], dataArray[3], decimalDigit);
        }
    }

    protected override void SetAnalog(byte position, byte value)
    {
        if ((Commands)position is
            Commands.SetBacklightBrightness or
            Commands.SetDisplayBrightness or
            Commands.SetBaroQnhIndicatorsBrightness or
            Commands.SetKeyIndicatorsBrightness or
            Commands.SetKeyBacklightBrightness or
            Commands.SetAutoLandIndicatorBrightness or
            Commands.SetMasterWarningIndicatorBrightness or
            Commands.SetMasterCautionIndicatorBrightness or
            Commands.SetArrowIndicatorBrightness or
            Commands.SetStickPriorityIndicatorBrightness)
        {
            SendCommand(position, value);
        }
    }

    protected override bool GetValue(byte responses)
    {
        switch ((Responses)responses)
        {
            case Responses.FdPressed:
            case Responses.IlsPressed:
            case Responses.CstrPressed:
            case Responses.WptPressed:
            case Responses.VorDPressed:
            case Responses.NdbPressed:
            case Responses.ArptPressed:
            case Responses.PushPressed:
            case Responses.PullPressed:
            case Responses.ChronoPressed:
            case Responses.MasterWarnPressed:
            case Responses.MasterCautPressed:
                OnSwitchPositionChanged(Buffer.Dequeue(), true);
                return true;

            case Responses.InHgSelected:
            case Responses.HPaSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.InHgSelected, (byte)Responses.HPaSelected);

            case Responses.Adf1Selected:
            case Responses.Off1Selected:
            case Responses.Vor1Selected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.Adf1Selected, (byte)Responses.Vor1Selected);

            case Responses.Adf2Selected:
            case Responses.Off2Selected:
            case Responses.Vor2Selected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.Adf2Selected, (byte)Responses.Vor2Selected);

            case Responses.ModeIlsSelected:
            case Responses.ModeVorSelected:
            case Responses.ModeNavSelected:
            case Responses.ModeArcSelected:
            case Responses.ModePlanSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.ModeIlsSelected, (byte)Responses.ModePlanSelected);

            case Responses.Range10Selected:
            case Responses.Range20Selected:
            case Responses.Range40Selected:
            case Responses.Range80Selected:
            case Responses.Range160Selected:
            case Responses.Range320Selected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.Range10Selected, (byte)Responses.Range320Selected);

            case Responses.EncoderIncremented:
            case Responses.EncoderDecremented:
                return HandleSwitchPositionChange(Buffer.Dequeue(), ref _skipNextEncoderFrame);

            case Responses.FdReleased:
            case Responses.IlsReleased:
            case Responses.CstrReleased:
            case Responses.WptReleased:
            case Responses.VorDReleased:
            case Responses.NdbReleased:
            case Responses.ArptReleased:
            case Responses.PushReleased:
            case Responses.PullReleased:
            case Responses.ChronoReleased:
            case Responses.MasterWarnReleased:
            case Responses.MasterCautReleased:
                OnSwitchPositionChanged(Buffer.Dequeue() - (byte)Responses.FdReleased, false);
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
        SetBaroQnhIndicatorsBrightness = 0x07,
        SetKeyIndicatorsBrightness = 0x08,
        SetKeyBacklightBrightness = 0x09,
        SetAutoLandIndicatorBrightness = 0x0A,
        SetMasterWarningIndicatorBrightness = 0x0B,
        SetMasterCautionIndicatorBrightness = 0x0C,
        SetArrowIndicatorBrightness = 0x0D,
        SetStickPriorityIndicatorBrightness = 0x0E,
        SetDisplay = 0x0F,
        SetIndicators = 0x10,
        ClearIndicators = 0x11,
        SetKeyIndicators = 0x12,
        ClearKeyIndicators = 0x13,
        RequestBaroSelectorPosition = 0x14,
        RequestModeSelectorPosition = 0x15,
        RequestRangeSelectorPosition = 0x16,
        RequestAdfVor1SelectorPosition = 0x17,
        RequestAdfVor2SelectorPosition = 0x18
    }

    private enum Responses : byte
    {
        FdPressed = 0x00,
        IlsPressed = 0x01,
        CstrPressed = 0x02,
        WptPressed = 0x03,
        VorDPressed = 0x04,
        NdbPressed = 0x05,
        ArptPressed = 0x06,
        PushPressed = 0x07,
        PullPressed = 0x08,
        ChronoPressed = 0x09,
        MasterWarnPressed = 0x0A,
        MasterCautPressed = 0x0B,
        InHgSelected = 0x0C,
        HPaSelected = 0x0D,
        Adf1Selected = 0x0E,
        Off1Selected = 0x0F,
        Vor1Selected = 0x10,
        Adf2Selected = 0x11,
        Off2Selected = 0x12,
        Vor2Selected = 0x13,
        ModeIlsSelected = 0x14,
        ModeVorSelected = 0x15,
        ModeNavSelected = 0x16,
        ModeArcSelected = 0x17,
        ModePlanSelected = 0x18,
        Range10Selected = 0x19,
        Range20Selected = 0x1A,
        Range40Selected = 0x1B,
        Range80Selected = 0x1C,
        Range160Selected = 0x1D,
        Range320Selected = 0x1E,
        EncoderIncremented = 0x1F,
        EncoderDecremented = 0x20,
        FdReleased = 0x40,
        IlsReleased = 0x41,
        CstrReleased = 0x42,
        WptReleased = 0x43,
        VorDReleased = 0x44,
        NdbReleased = 0x45,
        ArptReleased = 0x46,
        PushReleased = 0x47,
        PullReleased = 0x48,
        ChronoReleased = 0x49,
        MasterWarnReleased = 0x4A,
        MasterCautReleased = 0x4B
    }
}