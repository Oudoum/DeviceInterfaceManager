using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.Services.Devices.FsCockpit;

public class FsCockpitAirbusEcamSwService : FsCockpitServiceBase
{
    public FsCockpitAirbusEcamSwService(FsCockpitSerialPortService fsCockpitSerialPortService) : base(fsCockpitSerialPortService)
    {
        Inputs.Builder inputsBuilder = new();
        Outputs.Builder outputsBuilder = new();
        Inputs = inputsBuilder.SetSwitchInfo(0, 33).SetAnalogInfo(34, 35).Build();
        Outputs = outputsBuilder.SetLedInfo(0, 15).SetAnalogInfo(5, 7).Build();
    }

    protected override void StartupRequest()
    {
        SendCommand((byte)Commands.RequestAttHdgSelectorPosition);
        SendCommand((byte)Commands.RequestAirDataSelectorPosition);
        SendCommand((byte)Commands.RequestEisDmcSelectorPosition);
        SendCommand((byte)Commands.RequestEcamNdXfrSelectorPosition);
        SendCommand((byte)Commands.RequestUpperDisplayPotentiometerSwitch);
        SendCommand((byte)Commands.RequestLowerDisplayPotentiometerSwitch);
        SendCommand((byte)Commands.RequestUpperDisplayPotentiometerValue);
        SendCommand((byte)Commands.RequestLowerDisplayPotentiometerValue);
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
            Commands.SetKeyBacklightBrightness or
            Commands.SetKeyIndicatorsBrightness)
        {
            SendCommand(position, value);
        }
    }

    protected override bool GetValue(byte responses)
    {
        switch ((Responses)responses)
        {
            case Responses.EngPressed:
            case Responses.ApuPressed:
            case Responses.ClrPressed:
            case Responses.ToConfigPressed:
            case Responses.BleedPressed:
            case Responses.CondPressed:
            case Responses.PressPressed:
            case Responses.DoorPressed:
            case Responses.StsPressed:
            case Responses.ElecPressed:
            case Responses.WheelPressed:
            case Responses.RclPressed:
            case Responses.EmerCancPressed:
            case Responses.HydPressed:
            case Responses.FctlPressed:
            case Responses.FuelPressed:
            case Responses.AllPressed:
            case Responses.ClrTwoPressed:
                OnSwitchPositionChanged(Buffer.Dequeue(), true);
                return true;

            case Responses.UpperDispOffSelected:
            case Responses.UpperDispOnSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.UpperDispOffSelected, (byte)Responses.UpperDispOnSelected);

            case Responses.LowerDispOffSelected:
            case Responses.LowerDispOnSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.LowerDispOffSelected, (byte)Responses.LowerDispOnSelected);

            case Responses.AttHdgCptSelected:
            case Responses.AttHdgNormSelected:
            case Responses.AttHdgFoSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.AttHdgCptSelected, (byte)Responses.AttHdgFoSelected);

            case Responses.AirDataCptSelected:
            case Responses.AirDataNormSelected:
            case Responses.AirDataFoSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.AirDataCptSelected, (byte)Responses.AirDataFoSelected);

            case Responses.EisDmcCptSelected:
            case Responses.EisDmcNormSelected:
            case Responses.EisDmcFoSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.EisDmcCptSelected, (byte)Responses.EisDmcFoSelected);

            case Responses.EcamNdXfrCptSelected:
            case Responses.EcamNdXfrNormSelected:
            case Responses.EcamNdXfrFoSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.EcamNdXfrCptSelected, (byte)Responses.EcamNdXfrFoSelected);

            case Responses.UpperDispPotentiometerValue when Buffer.Count >= 2:
                return ProcessAnalogValueNormal((byte)Responses.UpperDispPotentiometerValue, GetFrame(2));

            case Responses.UpperDispPotentiometerValue:
                return false;

            case Responses.LowerDispPotentiometerValue when Buffer.Count >= 2:
                return ProcessAnalogValueNormal((byte)Responses.LowerDispPotentiometerValue, GetFrame(2));

            case Responses.LowerDispPotentiometerValue:
                return false;

            case Responses.EngReleased:
            case Responses.ApuReleased:
            case Responses.ClrReleased:
            case Responses.ToConfigReleased:
            case Responses.BleedReleased:
            case Responses.CondReleased:
            case Responses.PressReleased:
            case Responses.DoorReleased:
            case Responses.StsReleased:
            case Responses.ElecReleased:
            case Responses.WheelReleased:
            case Responses.RclReleased:
            case Responses.EmerCancReleased:
            case Responses.HydReleased:
            case Responses.FctlReleased:
            case Responses.FuelReleased:
            case Responses.AllReleased:
            case Responses.ClrTwoReleased:
                OnSwitchPositionChanged(Buffer.Dequeue() - (byte)Responses.EngReleased, false);
                return true;

            default:
                Buffer.TryDequeue(out byte _);
                return true;
        }
    }

    private enum Commands : byte
    {
        SetBacklightBrightness = 0x05,
        SetKeyBacklightBrightness = 0x06,
        SetKeyIndicatorsBrightness = 0x07,
        SetKeyIndicators = 0x08,
        ClearKeyIndicators = 0x09,
        RequestAttHdgSelectorPosition = 0x0A,
        RequestAirDataSelectorPosition = 0x0B,
        RequestEisDmcSelectorPosition = 0x0C,
        RequestEcamNdXfrSelectorPosition = 0x0D,
        RequestUpperDisplayPotentiometerSwitch = 0x0E,
        RequestLowerDisplayPotentiometerSwitch = 0x0F,
        RequestUpperDisplayPotentiometerValue = 0x10,
        RequestLowerDisplayPotentiometerValue = 0x11
    }

    private enum Responses : byte
    {
        EngPressed = 0x00,
        ApuPressed = 0x01,
        ClrPressed = 0x02,
        ToConfigPressed = 0x03,
        BleedPressed = 0x04,
        CondPressed = 0x05,
        PressPressed = 0x06,
        DoorPressed = 0x07,
        StsPressed = 0x08,
        ElecPressed = 0x09,
        WheelPressed = 0x0A,
        RclPressed = 0x0B,
        EmerCancPressed = 0x0C,
        HydPressed = 0x0D,
        FctlPressed = 0x0E,
        FuelPressed = 0x0F,
        AllPressed = 0x10,
        ClrTwoPressed = 0x11,
        UpperDispOffSelected = 0x12,
        UpperDispOnSelected = 0x13,
        LowerDispOffSelected = 0x14,
        LowerDispOnSelected = 0x15,
        AttHdgCptSelected = 0x16,
        AttHdgNormSelected = 0x17,
        AttHdgFoSelected = 0x18,
        AirDataCptSelected = 0x19,
        AirDataNormSelected = 0x1A,
        AirDataFoSelected = 0x1B,
        EisDmcCptSelected = 0x1C,
        EisDmcNormSelected = 0x1D,
        EisDmcFoSelected = 0x1E,
        EcamNdXfrCptSelected = 0x1F,
        EcamNdXfrNormSelected = 0x20,
        EcamNdXfrFoSelected = 0x21,
        UpperDispPotentiometerValue = 0x22,
        LowerDispPotentiometerValue = 0x23,
        EngReleased = 0x40,
        ApuReleased = 0x41,
        ClrReleased = 0x42,
        ToConfigReleased = 0x43,
        BleedReleased = 0x44,
        CondReleased = 0x45,
        PressReleased = 0x46,
        DoorReleased = 0x47,
        StsReleased = 0x48,
        ElecReleased = 0x49,
        WheelReleased = 0x4A,
        RclReleased = 0x4B,
        EmerCancReleased = 0x4C,
        HydReleased = 0x4D,
        FctlReleased = 0x4E,
        FuelReleased = 0x4F,
        AllReleased = 0x50,
        ClrTwoReleased = 0x51
    }
}