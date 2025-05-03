using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.Services.Devices.FsCockpit;

public class FsCockpitAirbusAirconOvhService : FsCockpitServiceBase
{
    public FsCockpitAirbusAirconOvhService(FsCockpitSerialPortService fsCockpitSerialPortService) : base(fsCockpitSerialPortService)
    {
        Inputs.Builder inputsBuilder = new();
        Outputs.Builder outputsBuilder = new();
        Inputs = inputsBuilder.SetSwitchInfo(0, 12).SetAnalogInfo(32, 34).Build();
        Outputs = outputsBuilder.SetLedInfo(0, 12).SetAnalogInfo(5, 8).Build();
    }

    protected override void StartupRequest()
    {
        SendCommand((byte)Commands.RequestPackFlowSelectorPosition);
        SendCommand((byte)Commands.RequestXBleedSelectorPosition);
        SendCommand((byte)Commands.RequestCockpitPotentiometerValue);
        SendCommand((byte)Commands.RequestFwdCabinPotentiometerValue);
        SendCommand((byte)Commands.RequestAftCabinPotentiometerValue);
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
            Commands.SetAmberIndicatorsBrightness or
            Commands.SetWhiteIndicatorsBrightness or
            Commands.SetBlueIndicatorsBrightness)
        {
            SendCommand(position, value);
        }
    }

    protected override bool GetValue(byte responses)
    {
        switch ((Responses)responses)
        {
            case Responses.HotAirPressed:
            case Responses.Pack1Pressed:
            case Responses.Pack2Pressed:
            case Responses.Eng1BleedPressed:
            case Responses.Eng2BleedPressed:
            case Responses.ApuBleedPressed:
            case Responses.RamAirPressed:
                OnSwitchPositionChanged(Buffer.Dequeue(), true);
                return true;

            case Responses.PackFlowLoSelected:
            case Responses.PackFlowNormSelected:
            case Responses.PackFlowHiSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.PackFlowLoSelected, (byte)Responses.PackFlowHiSelected);

            case Responses.XBleedShutSelected:
            case Responses.XBleedAutoSelected:
            case Responses.XBleedOpenSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.XBleedShutSelected, (byte)Responses.XBleedOpenSelected);

            case Responses.HotAirReleased:
            case Responses.Pack1Released:
            case Responses.Pack2Released:
            case Responses.Eng1BleedReleased:
            case Responses.Eng2BleedReleased:
            case Responses.ApuBleedReleased:
            case Responses.RamAirReleased:
                OnSwitchPositionChanged(Buffer.Dequeue() - (byte)Responses.HotAirReleased, false);
                return true;

            case Responses.CockpitPotentiometerValue when Buffer.Count >= 2:
                return ProcessAnalogValueNormal((byte)Responses.CockpitPotentiometerValue, GetFrame(2));

            case Responses.CockpitPotentiometerValue:
                return false;

            case Responses.FwdCabinPotentiometerValue when Buffer.Count >= 2:
                return ProcessAnalogValueNormal((byte)Responses.FwdCabinPotentiometerValue, GetFrame(2));

            case Responses.FwdCabinPotentiometerValue:
                return false;

            case Responses.AftCabinPotentiometerValue when Buffer.Count >= 2:
                return ProcessAnalogValueNormal((byte)Responses.AftCabinPotentiometerValue, GetFrame(2));

            case Responses.AftCabinPotentiometerValue:
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
        SetBlueIndicatorsBrightness = 0x08,
        SetKeyIndicators = 0x09,
        ClearKeyIndicators = 0x0A,
        RequestPackFlowSelectorPosition = 0x0B,
        RequestXBleedSelectorPosition = 0x0C,
        RequestCockpitPotentiometerValue = 0x0D,
        RequestFwdCabinPotentiometerValue = 0x0E,
        RequestAftCabinPotentiometerValue = 0x0F
    }

    private enum Responses : byte
    {
        HotAirPressed = 0x00,
        Pack1Pressed = 0x01,
        Pack2Pressed = 0x02,
        Eng1BleedPressed = 0x03,
        Eng2BleedPressed = 0x04,
        ApuBleedPressed = 0x05,
        RamAirPressed = 0x06,
        PackFlowLoSelected = 0x07,
        PackFlowNormSelected = 0x08,
        PackFlowHiSelected = 0x09,
        XBleedShutSelected = 0x0A,
        XBleedAutoSelected = 0x0B,
        XBleedOpenSelected = 0x0C,
        HotAirReleased = 0x10,
        Pack1Released = 0x11,
        Pack2Released = 0x12,
        Eng1BleedReleased = 0x13,
        Eng2BleedReleased = 0x14,
        ApuBleedReleased = 0x15,
        RamAirReleased = 0x16,
        CockpitPotentiometerValue = 0x20,
        FwdCabinPotentiometerValue = 0x21,
        AftCabinPotentiometerValue = 0x22,
    }
}