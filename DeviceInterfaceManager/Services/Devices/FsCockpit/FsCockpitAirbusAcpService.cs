using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.Services.Devices.FsCockpit;

public class FsCockpitAirbusAcpService : FsCockpitServiceBase
{
    public FsCockpitAirbusAcpService(FsCockpitSerialPortService fsCockpitSerialPortService) : base(fsCockpitSerialPortService)
    {
        Inputs.Builder inputsBuilder = new();
        Outputs.Builder outputsBuilder = new();
        Inputs = inputsBuilder.SetSwitchInfo(0, 27).SetAnalogInfo(64, 78).Build();
        Outputs = outputsBuilder.SetLedInfo(0, 7).SetDatalineInfo(0, 14).SetAnalogInfo(5, 8).Build();
    }

    protected override void StartupRequest()
    {
        SendCommand((byte)Commands.RequestPotentiometerValue, (byte)Responses.VhfOnePotentiometerValue);
        SendCommand((byte)Commands.RequestPotentiometerValue, (byte)Responses.VhfTwoPotentiometerValue);
        SendCommand((byte)Commands.RequestPotentiometerValue, (byte)Responses.VhfThreePotentiometerValue);
        SendCommand((byte)Commands.RequestPotentiometerValue, (byte)Responses.HfOnePotentiometerValue);
        SendCommand((byte)Commands.RequestPotentiometerValue, (byte)Responses.HfTwoPotentiometerValue);
        SendCommand((byte)Commands.RequestPotentiometerValue, (byte)Responses.IntPotentiometerValue);
        SendCommand((byte)Commands.RequestPotentiometerValue, (byte)Responses.CabPotentiometerValue);
        SendCommand((byte)Commands.RequestPotentiometerValue, (byte)Responses.PaPotentiometerValue);
        SendCommand((byte)Commands.RequestPotentiometerValue, (byte)Responses.VorOnePotentiometerValue);
        SendCommand((byte)Commands.RequestPotentiometerValue, (byte)Responses.VorTwoPotentiometerValue);
        SendCommand((byte)Commands.RequestPotentiometerValue, (byte)Responses.MkrPotentiometerValue);
        SendCommand((byte)Commands.RequestPotentiometerValue, (byte)Responses.IlsPotentiometerValue);
        SendCommand((byte)Commands.RequestPotentiometerValue, (byte)Responses.MlsPotentiometerValue);
        SendCommand((byte)Commands.RequestPotentiometerValue, (byte)Responses.AdfOnePotentiometerValue);
        SendCommand((byte)Commands.RequestPotentiometerValue, (byte)Responses.AdfTwoPotentiometerValue);
        SendCommand((byte)Commands.RequestLeverSwitchPosition);
        AdditionalStartupRequest();
    }

    protected virtual void AdditionalStartupRequest()
    {
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
        SendCommand((byte)Commands.SetPotentiometerIndicators, byte1, byte2);
    }

    protected override void ClearDataline(byte byte1, byte byte2, byte byte3, byte byte4)
    {
        SendCommand((byte)Commands.ClearPotentiometerIndicators, byte1, byte2);
    }

    protected override void SetDisplay(byte position, string data)
    {
    }

    protected override void SetAnalog(byte position, byte value)
    {
        if ((Commands)position is
            Commands.SetBacklightBrightness or
            Commands.SetKeyBacklightBrightness or
            Commands.SetKeyIndicatorsBrightness or
            Commands.SetPotentiometerIndicatorsBrightness)
        {
            SendCommand(position, value);
        }
    }

    protected override bool GetValue(byte responses)
    {
        switch ((Responses)responses)
        {
            case Responses.VhfOnePressed:
            case Responses.VhfTwoPressed:
            case Responses.VhfThreePressed:
            case Responses.HfOnePressed:
            case Responses.HhfTwoPressed:
            case Responses.IntPressed:
            case Responses.CabPressed:
            case Responses.PaPressed:
            case Responses.OnVoicePressed:
            case Responses.ResetPressed:
            case Responses.VhfOnePotPressed:
            case Responses.VhfZwoPotPressed:
            case Responses.VhfThreePotPressed:
            case Responses.HfOnePotPressed:
            case Responses.HfTwoPotPressed:
            case Responses.IntPotPressed:
            case Responses.CabPotPressed:
            case Responses.PaPotPressed:
            case Responses.VorOnePotPressed:
            case Responses.VorTwoPotPressed:
            case Responses.MkrPotPressed:
            case Responses.IlsPotPressed:
            case Responses.MlsPotPressed:
            case Responses.AdfOnePotPressed:
            case Responses.AdfTwoPotPressed:
                OnSwitchPositionChanged(Buffer.Dequeue(), true);
                return true;

            case Responses.IntSelected:
            case Responses.NeutralSelected:
            case Responses.RadSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.IntSelected, (byte)Responses.RadSelected);

            case Responses.VhfOneReleased:
            case Responses.VhfTwoReleased:
            case Responses.VhfThreeReleased:
            case Responses.HfOneReleased:
            case Responses.HhfTwoReleased:
            case Responses.IntReleased:
            case Responses.CabReleased:
            case Responses.PaReleased:
            case Responses.OnVoiceReleased:
            case Responses.ResetReleased:
            case Responses.VhfOnePotReleased:
            case Responses.VhfZwoPotReleased:
            case Responses.VhfThreePotReleased:
            case Responses.HfOnePotReleased:
            case Responses.HfTwoPotReleased:
            case Responses.IntPotReleased:
            case Responses.CabPotReleased:
            case Responses.PaPotReleased:
            case Responses.VorOnePotReleased:
            case Responses.VorTwoPotReleased:
            case Responses.MkrPotReleased:
            case Responses.IlsPotReleased:
            case Responses.MlsPotReleased:
            case Responses.AdfOnePotReleased:
            case Responses.AdfTwoPotReleased:
                OnSwitchPositionChanged(Buffer.Dequeue() - (byte)Responses.VhfOneReleased, false);
                return true;

            case Responses.VhfOnePotentiometerValue when Buffer.Count >= 2:
                return ProcessAnalogValueNormal((byte)Responses.VhfOnePotentiometerValue, GetFrame(2));

            case Responses.VhfOnePotentiometerValue:
                return false;

            case Responses.VhfTwoPotentiometerValue when Buffer.Count >= 2:
                return ProcessAnalogValueNormal((byte)Responses.VhfTwoPotentiometerValue, GetFrame(2));

            case Responses.VhfTwoPotentiometerValue:
                return false;

            case Responses.VhfThreePotentiometerValue when Buffer.Count >= 2:
                return ProcessAnalogValueNormal((byte)Responses.VhfThreePotentiometerValue, GetFrame(2));

            case Responses.VhfThreePotentiometerValue:
                return false;

            case Responses.HfOnePotentiometerValue when Buffer.Count >= 2:
                return ProcessAnalogValueNormal((byte)Responses.HfOnePotentiometerValue, GetFrame(2));

            case Responses.HfOnePotentiometerValue:
                return false;

            case Responses.HfTwoPotentiometerValue when Buffer.Count >= 2:
                return ProcessAnalogValueNormal((byte)Responses.HfTwoPotentiometerValue, GetFrame(2));

            case Responses.HfTwoPotentiometerValue:
                return false;

            case Responses.IntPotentiometerValue when Buffer.Count >= 2:
                return ProcessAnalogValueNormal((byte)Responses.IntPotentiometerValue, GetFrame(2));

            case Responses.IntPotentiometerValue:
                return false;

            case Responses.CabPotentiometerValue when Buffer.Count >= 2:
                return ProcessAnalogValueNormal((byte)Responses.CabPotentiometerValue, GetFrame(2));

            case Responses.CabPotentiometerValue:
                return false;

            case Responses.PaPotentiometerValue when Buffer.Count >= 2:
                return ProcessAnalogValueNormal((byte)Responses.PaPotentiometerValue, GetFrame(2));

            case Responses.PaPotentiometerValue:
                return false;

            case Responses.VorOnePotentiometerValue when Buffer.Count >= 2:
                return ProcessAnalogValueNormal((byte)Responses.VorOnePotentiometerValue, GetFrame(2));

            case Responses.VorOnePotentiometerValue:
                return false;

            case Responses.VorTwoPotentiometerValue when Buffer.Count >= 2:
                return ProcessAnalogValueNormal((byte)Responses.VorTwoPotentiometerValue, GetFrame(2));

            case Responses.VorTwoPotentiometerValue:
                return false;

            case Responses.MkrPotentiometerValue when Buffer.Count >= 2:
                return ProcessAnalogValueNormal((byte)Responses.MkrPotentiometerValue, GetFrame(2));

            case Responses.MkrPotentiometerValue:
                return false;

            case Responses.IlsPotentiometerValue when Buffer.Count >= 2:
                return ProcessAnalogValueNormal((byte)Responses.IlsPotentiometerValue, GetFrame(2));

            case Responses.IlsPotentiometerValue:
                return false;

            case Responses.MlsPotentiometerValue when Buffer.Count >= 2:
                return ProcessAnalogValueNormal((byte)Responses.MlsPotentiometerValue, GetFrame(2));

            case Responses.MlsPotentiometerValue:
                return false;

            case Responses.AdfOnePotentiometerValue when Buffer.Count >= 2:
                return ProcessAnalogValueNormal((byte)Responses.AdfOnePotentiometerValue, GetFrame(2));

            case Responses.AdfOnePotentiometerValue:
                return false;

            case Responses.AdfTwoPotentiometerValue when Buffer.Count >= 2:
                return ProcessAnalogValueNormal((byte)Responses.AdfTwoPotentiometerValue, GetFrame(2));

            case Responses.AdfTwoPotentiometerValue:
                return false;

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
        SetPotentiometerIndicatorsBrightness = 0x08,
        SetKeyIndicators = 0x09,
        ClearKeyIndicators = 0x0A,
        SetPotentiometerIndicators = 0x0B,
        ClearPotentiometerIndicators = 0x0C,
        RequestPotentiometerValue = 0x0D,
        RequestLeverSwitchPosition = 0x0E
    }

    private enum Responses : byte
    {
        VhfOnePressed = 0x00,
        VhfTwoPressed = 0x01,
        VhfThreePressed = 0x02,
        HfOnePressed = 0x03,
        HhfTwoPressed = 0x04,
        IntPressed = 0x05,
        CabPressed = 0x06,
        PaPressed = 0x07,
        OnVoicePressed = 0x08,
        ResetPressed = 0x09,
        VhfOnePotPressed = 0x0A,
        VhfZwoPotPressed = 0x0B,
        VhfThreePotPressed = 0x0C,
        HfOnePotPressed = 0x0D,
        HfTwoPotPressed = 0x0E,
        IntPotPressed = 0x0F,
        CabPotPressed = 0x10,
        PaPotPressed = 0x11,
        VorOnePotPressed = 0x12,
        VorTwoPotPressed = 0x13,
        MkrPotPressed = 0x14,
        IlsPotPressed = 0x15,
        MlsPotPressed = 0x16,
        AdfOnePotPressed = 0x17,
        AdfTwoPotPressed = 0x18,
        IntSelected = 0x19,
        NeutralSelected = 0x1A,
        RadSelected = 0x1B,
        VhfOneReleased = 0x20,
        VhfTwoReleased = 0x21,
        VhfThreeReleased = 0x22,
        HfOneReleased = 0x23,
        HhfTwoReleased = 0x24,
        IntReleased = 0x25,
        CabReleased = 0x26,
        PaReleased = 0x27,
        OnVoiceReleased = 0x28,
        ResetReleased = 0x29,
        VhfOnePotReleased = 0x2A,
        VhfZwoPotReleased = 0x2B,
        VhfThreePotReleased = 0x2C,
        HfOnePotReleased = 0x2D,
        HfTwoPotReleased = 0x2E,
        IntPotReleased = 0x2F,
        CabPotReleased = 0x30,
        PaPotReleased = 0x31,
        VorOnePotReleased = 0x32,
        VorTwoPotReleased = 0x33,
        MkrPotReleased = 0x34,
        IlsPotReleased = 0x35,
        MlsPotReleased = 0x36,
        AdfOnePotReleased = 0x37,
        AdfTwoPotReleased = 0x38,
        VhfOnePotentiometerValue = 0x40,
        VhfTwoPotentiometerValue = 0x41,
        VhfThreePotentiometerValue = 0x42,
        HfOnePotentiometerValue = 0x43,
        HfTwoPotentiometerValue = 0x44,
        IntPotentiometerValue = 0x45,
        CabPotentiometerValue = 0x46,
        PaPotentiometerValue = 0x47,
        VorOnePotentiometerValue = 0x48,
        VorTwoPotentiometerValue = 0x49,
        MkrPotentiometerValue = 0x4A,
        IlsPotentiometerValue = 0x4B,
        MlsPotentiometerValue = 0x4C,
        AdfOnePotentiometerValue = 0x4D,
        AdfTwoPotentiometerValue = 0x4E,
    }
}