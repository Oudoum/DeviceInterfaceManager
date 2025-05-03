using System.Collections.Generic;
using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.Services.Devices.FsCockpit;

public class FsCockpitAirbusTpbpService : FsCockpitServiceBase
{
    public FsCockpitAirbusTpbpService(FsCockpitSerialPortService fsCockpitSerialPortService) : base(fsCockpitSerialPortService)
    {
        Inputs.Builder inputsBuilder = new();
        Outputs.Builder outputsBuilder = new();
        Inputs = inputsBuilder.SetSwitchInfo(0, 14).SetAnalogInfo(11, 12).Build();
        Outputs = outputsBuilder.SetLedInfo(0, 3).SetSevenSegmentInfo(9, 9).SetAnalogInfo(5, 8).Build();
    }

    protected override void StartupRequest()
    {
        SendCommand((byte)Commands.RequestParkingBrake);
        SendCommand((byte)Commands.RequestSpeedBrakeArmedSelector);
        SendCommand((byte)Commands.RequestCockpitDoorSelector);
        SendCommand((byte)Commands.RequestSpeedBrakeLeverValue);
        SendCommand((byte)Commands.RequestFlapsLeverValue);
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
    }

    protected override void ClearDataline(byte byte1, byte byte2, byte byte3, byte byte4)
    {
    }

    protected override void SetDisplay(byte position, string data)
    {
        byte[] dataArray = GetDisplayData(DisplayDigitCodes, data, out byte decimalDigit);

        if ((Commands)position == Commands.SetDisplay)
        {
            SendCommand(position, dataArray[0], dataArray[1], dataArray[2], decimalDigit);
        }
    }

    protected override void SetAnalog(byte position, byte value)
    {
        if ((Commands)position is
            Commands.SetBacklightBrightness or
            Commands.SetDisplayBrightness or
            Commands.SetLrIndicatorsBrightness or
            Commands.SetCockpitDoorDataLoaderIndicatorsBrightness)
        {
            SendCommand(position, value);
        }
    }

    protected override bool GetValue(byte responses)
    {
        switch ((Responses)responses)
        {
            case Responses.RudTrimResetPressed:
            case Responses.RudTrimNoseLPressed:
            case Responses.RudTrimNoseRPressed:
            case Responses.VideoPressed:
                OnSwitchPositionChanged(Buffer.Dequeue(), true);
                return true;

            case Responses.ParkingBrakeOn:
            case Responses.ParkingBrakeOff:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.ParkingBrakeOn, (byte)Responses.ParkingBrakeOff);

            case Responses.SpeedBrakeArmed:
            case Responses.SpeedBrakeDisarmed:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.SpeedBrakeArmed, (byte)Responses.SpeedBrakeDisarmed);

            case Responses.CockpitDoorUnlockSelected:
            case Responses.CockpitDoorNormSelected:
            case Responses.CockpitDoorLockSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.CockpitDoorUnlockSelected, (byte)Responses.CockpitDoorLockSelected);

            case Responses.SpeedBrakeLeverValue when Buffer.Count >= 2:
                return ProcessAnalogValueNormal((byte)Responses.SpeedBrakeLeverValue, GetFrame(2));

            case Responses.SpeedBrakeLeverValue:
                return false;

            case Responses.FlapsLeverValue when Buffer.Count >= 2:
                return ProcessAnalogValueNormal((byte)Responses.FlapsLeverValue, GetFrame(2));

            case Responses.FlapsLeverValue:
                return false;

            case Responses.GravityGearExtnRotationCw:
            case Responses.GravityGearExtnRotationCcw:
                return HandleSwitchPositionChange(Buffer.Dequeue());

            case Responses.RudTrimResetReleased:
            case Responses.RudTrimNoseLReleased:
            case Responses.RudTrimNoseRReleased:
            case Responses.VideoReleased:
                OnSwitchPositionChanged(Buffer.Dequeue() - (byte)Responses.RudTrimResetReleased, false);
                return true;

            default:
                Buffer.TryDequeue(out byte _);
                return true;
        }
    }

    private static readonly Dictionary<char, byte> DisplayDigitCodes = new()
    {
        { '0', 0x00 },
        { '1', 0x01 },
        { '2', 0x02 },
        { '3', 0x03 },
        { '4', 0x04 },
        { '5', 0x05 },
        { '6', 0x06 },
        { '7', 0x07 },
        { '8', 0x08 },
        { '9', 0x09 },
        { ' ', 0x0A },
        { '-', 0x0B }
    };

    private enum Commands : byte
    {
        SetBacklightBrightness = 0x05,
        SetDisplayBrightness = 0x06,
        SetLrIndicatorsBrightness = 0x07,
        SetCockpitDoorDataLoaderIndicatorsBrightness = 0x08,
        SetDisplay = 0x09,
        SetIndicators = 0x0A,
        ClearIndicators = 0x0B,
        RequestParkingBrake = 0x0C,
        RequestSpeedBrakeArmedSelector = 0x0D,
        RequestCockpitDoorSelector = 0x0E,
        RequestSpeedBrakeLeverValue = 0x0F,
        RequestFlapsLeverValue = 0x10
    }

    private enum Responses : byte
    {
        RudTrimResetPressed = 0x00,
        RudTrimNoseLPressed = 0x01,
        RudTrimNoseRPressed = 0x02,
        VideoPressed = 0x03,
        ParkingBrakeOn = 0x04,
        ParkingBrakeOff = 0x05,
        SpeedBrakeArmed = 0x06,
        SpeedBrakeDisarmed = 0x07,
        CockpitDoorUnlockSelected = 0x08,
        CockpitDoorNormSelected = 0x09,
        CockpitDoorLockSelected = 0x0A,
        SpeedBrakeLeverValue = 0x0B,
        FlapsLeverValue = 0x0C,
        GravityGearExtnRotationCw = 0x0D,
        GravityGearExtnRotationCcw = 0x0E,
        RudTrimResetReleased = 0x10,
        RudTrimNoseLReleased = 0x11,
        RudTrimNoseRReleased = 0x12,
        VideoReleased = 0x13
    }
}