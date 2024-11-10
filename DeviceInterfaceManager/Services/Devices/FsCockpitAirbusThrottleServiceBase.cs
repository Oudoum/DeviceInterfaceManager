using System;
using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.Services.Devices;

public class FsCockpitAirbusThrottleServiceBase : FsCockpitServiceBase
{
    private byte _trimSpeed;

    public FsCockpitAirbusThrottleServiceBase()
    {
        Inputs.Builder inputsBuilder = new();
        Outputs.Builder outputsBuilder = new();
        Inputs = inputsBuilder.SetSwitchInfo(0, 8).SetAnalogInfo(1, 3).Build();
        Outputs = outputsBuilder.SetLedInfo(1, 4).SetDatalineInfo(1, 1).SetAnalogInfo(5, 9).Build();
    }

    protected override ConnectionStatus InitialRequest()
    {
        while (string.IsNullOrEmpty(DeviceName))
        {
        }

        if (DeviceName != SerialNumber["AB15"])
        {
            return ConnectionStatus.NotConnected;
        }

        SendData((byte)ThrottleCommands.RequestMaster1SwitchPosition);
        SendData((byte)ThrottleCommands.RequestMaster2SwitchPosition);
        SendData((byte)ThrottleCommands.RequestEngineModeRotarySwitchPosition);
        SendData((byte)ThrottleCommands.RequestThrottle1Value);
        SendData((byte)ThrottleCommands.RequestThrottle2Value);
        SendData((byte)ThrottleCommands.RequestTrimmerValue);
        return ConnectionStatus.Connected;
    }

    protected override void SetIndicator(byte position)
    {
        SendData((byte)ThrottleCommands.SetIndicators, position);
    }

    protected override void ClearIndicator(byte position)
    {
        SendData((byte)ThrottleCommands.ClearIndicators, position);
    }

    protected override void SetDataline(int position, bool isEnabled)
    {
        if (position == 1 && isEnabled)
        {
            SendData((byte)ThrottleCommands.TrimmerStopNow);
        }
    }

    protected override void SetAnalog(byte position, byte value)
    {
        switch (position)
        {
            case < 8:
                SendData(position, value);
                break;

            case 8:
                byte crc8 = (byte)(0x08 ^ _trimSpeed ^ value);
                SendData(position, value, _trimSpeed, crc8);
                break;

            case 9:
                _trimSpeed = value;
                break;
        }
    }

    protected override bool GetValue(byte responses)
    {
        switch ((ThrottleResponses)responses)
        {
            case ThrottleResponses.AutoThrustDisableCptPressed:
            case ThrottleResponses.AutoThrustDisableFoPressed:
                OnSwitchPositionChanged(Buffer.Dequeue(), true);
                return true;

            case ThrottleResponses.Master1OffSelected:
                byte master1Off = Buffer.Dequeue();
                OnSwitchPositionChanged(master1Off + 0x01, false);
                OnSwitchPositionChanged(master1Off, true);
                return true;

            case ThrottleResponses.Master1OnSelected:
                byte master1On = Buffer.Dequeue();
                OnSwitchPositionChanged(master1On - 0x01, false);
                OnSwitchPositionChanged(master1On, true);
                return true;

            case ThrottleResponses.Master2OffSelected:
                byte master2Off = Buffer.Dequeue();
                OnSwitchPositionChanged(master2Off + 0x01, false);
                OnSwitchPositionChanged(master2Off, true);
                return true;

            case ThrottleResponses.Master2OnSelected:
                byte master2On = Buffer.Dequeue();
                OnSwitchPositionChanged(master2On - 0x01, false);
                OnSwitchPositionChanged(master2On, true);
                return true;

            case ThrottleResponses.EngineCrankSelected:
                byte engineCrank = Buffer.Dequeue();
                OnSwitchPositionChanged(engineCrank + 0x01, false);
                OnSwitchPositionChanged(engineCrank + 0x02, false);
                OnSwitchPositionChanged(engineCrank, true);
                return true;

            case ThrottleResponses.EngineNormalSelected:
                byte engineNormal = Buffer.Dequeue();
                OnSwitchPositionChanged(engineNormal - 0x01, false);
                OnSwitchPositionChanged(engineNormal + 0x01, false);
                OnSwitchPositionChanged(engineNormal, true);
                return true;

            case ThrottleResponses.EngineIgnStartSelected:
                byte engineIgnStart = Buffer.Dequeue();
                OnSwitchPositionChanged(engineIgnStart - 0x01, false);
                OnSwitchPositionChanged(engineIgnStart - 0x02, false);
                OnSwitchPositionChanged(engineIgnStart, true);
                return true;

            case ThrottleResponses.AutoThrustDisableCptReleased:
            case ThrottleResponses.AutoThrustDisableFoReleased:
                OnSwitchPositionChanged(Buffer.Dequeue() - 0x10, false);
                return true;

            case ThrottleResponses.Throttle1Value when Buffer.Count >= 2:
                ProcessThrottle1(GetFrame(2));
                return true;

            case ThrottleResponses.Throttle1Value:
                return false;

            case ThrottleResponses.Throttle2Value when Buffer.Count >= 2:
                ProcessThrottle2(GetFrame(2));
                return true;

            case ThrottleResponses.Throttle2Value:
                return false;

            case ThrottleResponses.TrimmerValue when Buffer.Count >= 2:
                ProcessTrimmerValue(GetFrame(2));
                return true;

            case ThrottleResponses.TrimmerValue:
                return false;

            case ThrottleResponses.TrimmerRotationCompleted:
                Buffer.Dequeue();
                return true;
            
            default:
                Buffer.TryDequeue(out byte _);
                return true;
        }
    }

    private void ProcessThrottle1(byte[] frame)
    {
        OnAnalogInValueChanged(1, (sbyte)frame[1]);
    }

    private void ProcessThrottle2(byte[] frame)
    {
        OnAnalogInValueChanged(2, (sbyte)frame[1]);
    }

    private void ProcessTrimmerValue(byte[] frame)
    {
        OnAnalogInValueChanged(3, (sbyte)frame[1]);
    }

    private enum ThrottleCommands
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
        ResetCommunicationStateMachine = 0xFF
    }

    [Flags]
    private enum ThrottleIndicator
    {
        Fault1 = 0x01,
        Fault2 = 0x02,
        Fire1 = 0x04,
        Fire2 = 0x08
    }

    private enum ThrottleResponses
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