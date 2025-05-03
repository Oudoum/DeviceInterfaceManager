using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.Services.Devices.FsCockpit;

public class FsCockpitAirbusThrottleService : FsCockpitServiceBase
{
    private byte _trimSpeed;

    public FsCockpitAirbusThrottleService(FsCockpitSerialPortService fsCockpitSerialPortService) : base(fsCockpitSerialPortService)
    {
        Inputs.Builder inputsBuilder = new();
        Outputs.Builder outputsBuilder = new();
        Inputs = inputsBuilder.SetSwitchInfo(0, 8).SetAnalogInfo(32, 34).Build();
        Outputs = outputsBuilder.SetLedInfo(0, 3).SetDatalineInfo(1, 1).SetAnalogInfo(5, 9).Build();
    }

    protected override void StartupRequest()
    {
        SendCommand((byte)Commands.RequestMaster1SwitchPosition);
        SendCommand((byte)Commands.RequestMaster2SwitchPosition);
        SendCommand((byte)Commands.RequestEngineModeRotarySwitchPosition);
        SendCommand((byte)Commands.RequestThrottle1Value);
        SendCommand((byte)Commands.RequestThrottle2Value);
        SendCommand((byte)Commands.RequestTrimmerValue);
    }

    protected override void SetLed(byte byte1, byte byte2, byte byte3, byte byte4)
    {
        SendCommand((byte)Commands.SetIndicators, byte1);
    }

    protected override void ClearLed(byte byte1, byte byte2, byte byte3, byte byte4)
    {
        SendCommand((byte)Commands.ClearIndicators, byte1);
    }

    protected override void SetDataline(byte byte1, byte byte2, byte byte3, byte byte4)
    {
        if (byte1 == 2)
        {
            SendCommand((byte)Commands.TrimmerStopNow);
        }
    }

    protected override void ClearDataline(byte byte1, byte byte2, byte byte3, byte byte4)
    {
    }

    protected override void SetDisplay(byte position, string data)
    {
    }

    protected override void SetAnalog(byte position, byte value)
    {
        switch ((Commands)position)
        {
            case Commands.SetBacklightBrightness:
            case Commands.SetFireIndicatorsBrightness:
            case Commands.SetFaultIndicatorsBrightness:
                SendCommand(position, value);
                break;

            case Commands.SetTrimmer:
                if (value is > 99 and < 220)
                {
                    value = 99;
                }
                byte crc8 = (byte)((byte)Commands.SetTrimmer ^ _trimSpeed ^ value);
                SendCommand(position, value, _trimSpeed, crc8);
                break;

            case (Commands)9:
                _trimSpeed = value;
                break;
        }
    }

    protected override bool GetValue(byte responses)
    {
        switch ((Responses)responses)
        {
            case Responses.AutoThrustDisableCptPressed:
            case Responses.AutoThrustDisableFoPressed:
                OnSwitchPositionChanged(Buffer.Dequeue(), true);
                return true;

            case Responses.Master1OffSelected:
            case Responses.Master1OnSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.Master1OffSelected, (byte)Responses.Master1OnSelected);

            case Responses.Master2OffSelected:
            case Responses.Master2OnSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.Master2OffSelected, (byte)Responses.Master2OnSelected);

            case Responses.EngineCrankSelected:
            case Responses.EngineNormalSelected:
            case Responses.EngineIgnStartSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.EngineCrankSelected, (byte)Responses.EngineIgnStartSelected);

            case Responses.AutoThrustDisableCptReleased:
            case Responses.AutoThrustDisableFoReleased:
                OnSwitchPositionChanged(Buffer.Dequeue() - (byte)Responses.AutoThrustDisableCptReleased, false);
                return true;

            case Responses.Throttle1Value when Buffer.Count >= 2:
                return ProcessAnalogValue((byte)Responses.Throttle1Value, GetFrame(2));

            case Responses.Throttle1Value:
                return false;

            case Responses.Throttle2Value when Buffer.Count >= 2:
                return ProcessAnalogValue((byte)Responses.Throttle2Value, GetFrame(2));

            case Responses.Throttle2Value:
                return false;

            case Responses.TrimmerValue when Buffer.Count >= 2:
                return ProcessAnalogValue((byte)Responses.TrimmerValue, GetFrame(2));

            case Responses.TrimmerValue:
                return false;

            case Responses.TrimmerRotationCompleted:
                Buffer.Dequeue();
                return true;

            default:
                Buffer.TryDequeue(out byte _);
                return true;
        }
    }

    private enum Commands : byte
    {
        SetBacklightBrightness = 0x05,
        SetFireIndicatorsBrightness = 0x06,
        SetFaultIndicatorsBrightness = 0x07,
        SetTrimmer = 0x08,
        TrimmerStopNow = 0x09,
        SetIndicators = 0x0A,
        ClearIndicators = 0x0B,
        RequestMaster1SwitchPosition = 0x0C,
        RequestMaster2SwitchPosition = 0x0D,
        RequestEngineModeRotarySwitchPosition = 0x0E,
        RequestThrottle1Value = 0x0F,
        RequestThrottle2Value = 0x10,
        RequestTrimmerValue = 0x11,
    }

    private enum Responses : byte
    {
        AutoThrustDisableCptPressed = 0x00,
        AutoThrustDisableFoPressed = 0x01,
        Master1OffSelected = 0x02,
        Master1OnSelected = 0x03,
        Master2OffSelected = 0x04,
        Master2OnSelected = 0x05,
        EngineCrankSelected = 0x06,
        EngineNormalSelected = 0x07,
        EngineIgnStartSelected = 0x08,
        AutoThrustDisableCptReleased = 0x10,
        AutoThrustDisableFoReleased = 0x11,
        Throttle1Value = 0x20,
        Throttle2Value = 0x21,
        TrimmerValue = 0x22,
        TrimmerRotationCompleted = 0x40,
    }
}