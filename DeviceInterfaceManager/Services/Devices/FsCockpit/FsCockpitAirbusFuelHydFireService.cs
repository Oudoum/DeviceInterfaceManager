using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.Services.Devices.FsCockpit;

public class FsCockpitAirbusFuelHydFireService : FsCockpitServiceBase
{
    public FsCockpitAirbusFuelHydFireService(FsCockpitSerialPortService fsCockpitSerialPortService) : base(fsCockpitSerialPortService)
    {
        Inputs.Builder inputsBuilder = new();
        Outputs.Builder outputsBuilder = new();
        Inputs = inputsBuilder.SetSwitchInfo(0, 33).Build();
        Outputs = outputsBuilder.SetLedInfo(0, 27).SetDatalineInfo(0, 12).SetAnalogInfo(5, 10).Build();
    }

    protected override void StartupRequest()
    {
        SendCommand((byte)Commands.RequestEng1FireSwitchPosition);
        SendCommand((byte)Commands.RequestEng2FireSwitchPosition);
        SendCommand((byte)Commands.RequestApuFireSwitchPosition);
        SendCommand((byte)Commands.RequestEng1FireLockPosition);
        SendCommand((byte)Commands.RequestEng2FireLockPosition);
        SendCommand((byte)Commands.RequestApuFireLockPosition);
    }

    protected override void SetLed(byte byte1, byte byte2, byte byte3, byte byte4)
    {
        SendCommand((byte)Commands.SetFuelHydKeyIndicators, byte1, byte2, byte3, byte4);
    }

    protected override void ClearLed(byte byte1, byte byte2, byte byte3, byte byte4)
    {
        SendCommand((byte)Commands.ClearFuelHydKeyIndicators, byte1, byte2, byte3, byte4);
    }

    protected override void SetDataline(byte byte1, byte byte2, byte byte3, byte byte4)
    {
        SendCommand((byte)Commands.SetFireKeyIndicators, byte1, byte2);
    }

    protected override void ClearDataline(byte byte1, byte byte2, byte byte3, byte byte4)
    {
        SendCommand((byte)Commands.ClearFireKeyIndicators, byte1, byte2);
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
            Commands.SetBlueIndicatorsBrightness or
            Commands.SetGreenIndicatorsBrightness)
        {
            SendCommand(position, value);
        }
    }

    protected override bool GetValue(byte responses)
    {
        switch ((Responses)responses)
        {
            case Responses.Eng1Agent1Pressed:
            case Responses.Eng1Agent2Pressed:
            case Responses.Eng1TestPressed:
            case Responses.Eng2Agent1Pressed:
            case Responses.Eng2Agent2Pressed:
            case Responses.Eng2TestPressed:
            case Responses.ApuAgentPressed:
            case Responses.ApuTestPressed:
            case Responses.Eng1PumpPressed:
            case Responses.RatManOnPressed:
            case Responses.BlueElecPumpPressed:
            case Responses.PtuPressed:
            case Responses.Eng2PumpPressed:
            case Responses.YellowElecPumpPressed:
            case Responses.XFeedPressed:
            case Responses.LTkPump1Pressed:
            case Responses.LTkPump2Pressed:
            case Responses.CtrTkPump1Pressed:
            case Responses.CtrTkModeSelPressed:
            case Responses.CtrTkPump2Pressed:
            case Responses.RTkPump1Pressed:
            case Responses.RTkPump2Pressed:
                OnSwitchPositionChanged(Buffer.Dequeue(), true);
                return true;

            case Responses.Eng1FireArmed:
            case Responses.Eng1FireDisarmed:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.Eng1FireArmed, (byte)Responses.Eng1FireDisarmed);

            case Responses.Eng2FireArmed:
            case Responses.Eng2FireDisarmed:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.Eng2FireArmed, (byte)Responses.Eng2FireDisarmed);

            case Responses.ApuFireArmed:
            case Responses.ApuFireDisarmed:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.ApuFireArmed, (byte)Responses.ApuFireDisarmed);

            case Responses.Eng1FireLocked:
            case Responses.Eng1FireUnlocked:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.Eng1FireLocked, (byte)Responses.Eng1FireUnlocked);

            case Responses.Eng2FireLocked:
            case Responses.Eng2FireUnlocked:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.Eng2FireLocked, (byte)Responses.Eng2FireUnlocked);

            case Responses.ApuFireLocked:
            case Responses.ApuFireUnlocked:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.ApuFireLocked, (byte)Responses.ApuFireUnlocked);

            case Responses.Eng1Agent1Released:
            case Responses.Eng1Agent2Released:
            case Responses.Eng1TestReleased:
            case Responses.Eng2Agent1Released:
            case Responses.Eng2Agent2Released:
            case Responses.Eng2TestReleased:
            case Responses.ApuAgentReleased:
            case Responses.ApuTestReleased:
            case Responses.Eng1PumpReleased:
            case Responses.RatManOnReleased:
            case Responses.BlueElecPumpReleased:
            case Responses.PtuReleased:
            case Responses.Eng2PumpReleased:
            case Responses.YellowElecPumpReleased:
            case Responses.XFeedReleased:
            case Responses.LTkPump1Released:
            case Responses.LTkPump2Released:
            case Responses.CtrTkPump1Released:
            case Responses.CtrTkModeSelReleased:
            case Responses.CtrTkPump2Released:
            case Responses.RTkPump1Released:
            case Responses.RTkPump2Released:
                OnSwitchPositionChanged(Buffer.Dequeue() - (byte)Responses.Eng1Agent1Released, false);
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
        SetGreenIndicatorsBrightness = 0x0A,
        SetFuelHydKeyIndicators = 0x0B,
        ClearFuelHydKeyIndicators = 0x0C,
        SetFireKeyIndicators = 0x0D,
        ClearFireKeyIndicators = 0x0E,
        RequestEng1FireSwitchPosition = 0x0F,
        RequestEng2FireSwitchPosition = 0x10,
        RequestApuFireSwitchPosition = 0x11,
        RequestEng1FireLockPosition = 0x12,
        RequestEng2FireLockPosition = 0x13,
        RequestApuFireLockPosition = 0x14
    }

    private enum Responses : byte
    {
        Eng1Agent1Pressed = 0x00,
        Eng1Agent2Pressed = 0x01,
        Eng1TestPressed = 0x02,
        Eng2Agent1Pressed = 0x03,
        Eng2Agent2Pressed = 0x04,
        Eng2TestPressed = 0x05,
        ApuAgentPressed = 0x06,
        ApuTestPressed = 0x07,
        Eng1PumpPressed = 0x08,
        RatManOnPressed = 0x09,
        BlueElecPumpPressed = 0x0A,
        PtuPressed = 0x0B,
        Eng2PumpPressed = 0x0C,
        YellowElecPumpPressed = 0x0D,
        XFeedPressed = 0x0E,
        LTkPump1Pressed = 0x0F,
        LTkPump2Pressed = 0x10,
        CtrTkPump1Pressed = 0x11,
        CtrTkModeSelPressed = 0x12,
        CtrTkPump2Pressed = 0x13,
        RTkPump1Pressed = 0x14,
        RTkPump2Pressed = 0x15,
        Eng1FireArmed = 0x16,
        Eng1FireDisarmed = 0x17,
        Eng2FireArmed = 0x18,
        Eng2FireDisarmed = 0x19,
        ApuFireArmed = 0x1A,
        ApuFireDisarmed = 0x1B,
        Eng1FireLocked = 0x1C,
        Eng1FireUnlocked = 0x1D,
        Eng2FireLocked = 0x1E,
        Eng2FireUnlocked = 0x1F,
        ApuFireLocked = 0x20,
        ApuFireUnlocked = 0x21,
        Eng1Agent1Released = 0x40,
        Eng1Agent2Released = 0x41,
        Eng1TestReleased = 0x42,
        Eng2Agent1Released = 0x43,
        Eng2Agent2Released = 0x44,
        Eng2TestReleased = 0x45,
        ApuAgentReleased = 0x46,
        ApuTestReleased = 0x47,
        Eng1PumpReleased = 0x48,
        RatManOnReleased = 0x49,
        BlueElecPumpReleased = 0x4A,
        PtuReleased = 0x4B,
        Eng2PumpReleased = 0x4C,
        YellowElecPumpReleased = 0x4D,
        XFeedReleased = 0x4E,
        LTkPump1Released = 0x4F,
        LTkPump2Released = 0x50,
        CtrTkPump1Released = 0x51,
        CtrTkModeSelReleased = 0x52,
        CtrTkPump2Released = 0x53,
        RTkPump1Released = 0x54,
        RTkPump2Released = 0x55
    }
}