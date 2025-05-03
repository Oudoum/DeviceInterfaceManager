using System.Collections.Generic;
using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.Services.Devices.FsCockpit;

public class FsCockpitAirbusAbLgsPtiClkService : FsCockpitServiceBase
{
    public FsCockpitAirbusAbLgsPtiClkService(FsCockpitSerialPortService fsCockpitSerialPortService) : base(fsCockpitSerialPortService)
    {
        Inputs.Builder inputsBuilder = new();
        Outputs.Builder outputsBuilder = new();
        Inputs = inputsBuilder.SetSwitchInfo(0, 30).Build();
        Outputs = outputsBuilder.SetLedInfo(0, 19).SetSevenSegmentInfo(23, 25).SetAnalogInfo(5, 22).Build();
    }

    protected override void StartupRequest()
    {
        SendCommand((byte)Commands.RequestAntiSkidSwitchPosition);
        SendCommand((byte)Commands.RequestLandingGearLeverSwitchPosition);
        SendCommand((byte)Commands.RequestClockUtcSelectorPosition);
        SendCommand((byte)Commands.RequestClockEtSelectorPosition);
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
        byte[] dataArray = GetDisplayData(DisplayDigitCodes, data, out byte decimalDigit);

        switch ((Commands)position)
        {
            case Commands.SetClockChrDisplay:
            case Commands.SetClockEtDisplay:
                SendCommand(position, dataArray[0], dataArray[1], dataArray[2], dataArray[3], dataArray[4], decimalDigit);
                break;

            case Commands.SetClockUtcDisplay:
                SendCommand(position, dataArray[0], dataArray[1], dataArray[2], dataArray[3], dataArray[4], dataArray[5], dataArray[6], decimalDigit);
                break;
        }
    }

    protected override void SetAnalog(byte position, byte value)
    {
        if ((Commands)position is
            Commands.SetBacklightBrightness or
            Commands.SetAmberIndicatorsBrightness or
            Commands.SetWhiteIndicatorsBrightness or
            Commands.SetRedIndicatorsBrightness or
            Commands.SetGreenIndicatorsBrightness or
            Commands.SetBlueIndicatorsBrightness or
            Commands.SetMipFloodLightBrightness or
            Commands.SetAccumulatorPressureIndicatorPosition or
            Commands.SetLeftBrakePressureIndicatorPosition or
            Commands.SetRightBrakePressureIndicatorPosition or
            Commands.SetClockDisplayBrightness)
        {
            SendCommand(position, value);
        }
    }

    protected override bool GetValue(byte responses)
    {
        switch ((Responses)responses)
        {
            case Responses.BrkFanPressed:
            case Responses.AutoBrkLoPressed:
            case Responses.AutoBrkMedPressed:
            case Responses.AutoBrkMaxPressed:
            case Responses.TerrOnNdFoPressed:
            case Responses.TerrOnNdCptPressed:
            case Responses.ClockDateSetPressed:
            case Responses.ClockRstPressed:
            case Responses.ClockChrPressed:
            case Responses.IsisBugsPressed:
            case Responses.IsisLsPressed:
            case Responses.IsisPlusPressed:
            case Responses.IsisMinusPressed:
            case Responses.IsisRstPressed:
            case Responses.IsisBaroPressed:
                OnSwitchPositionChanged(Buffer.Dequeue(), true);
                return true;

            case Responses.AntiSkidOnSelected:
            case Responses.AntiSkidOffSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.AntiSkidOnSelected, (byte)Responses.AntiSkidOffSelected);

            case Responses.LandingGearLeverUpSelected:
            case Responses.LandingGearLeverDownSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.LandingGearLeverUpSelected, (byte)Responses.LandingGearLeverDownSelected);

            case Responses.ClockUtcGpsSelected:
            case Responses.ClockUtcIntSelected:
            case Responses.ClockUtcSetSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.ClockUtcGpsSelected, (byte)Responses.ClockUtcSetSelected);

            case Responses.ClockEtRunSelected:
            case Responses.ClockEtStpSelected:
            case Responses.ClockEtRstSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.ClockEtRunSelected, (byte)Responses.ClockEtRstSelected);

            case Responses.ClockDateSetEncoderIncremented:
            case Responses.ClockDateSetEncoderDecremented:
            case Responses.IsisBaroEncoderIncremented:
            case Responses.IsisBaroEncoderDecremented:
                return HandleSwitchPositionChange(Buffer.Dequeue());

            case Responses.BrkFanReleased:
            case Responses.AutoBrkLoReleased:
            case Responses.AutoBrkMedReleased:
            case Responses.AutoBrkMaxReleased:
            case Responses.TerrOnNdFoReleased:
            case Responses.TerrOnNdCptReleased:
            case Responses.ClockDateSetReleased:
            case Responses.ClockRstReleased:
            case Responses.ClockChrReleased:
            case Responses.IsisBugsReleased:
            case Responses.IsisLsReleased:
            case Responses.IsisPlusReleased:
            case Responses.IsisMinusReleased:
            case Responses.IsisRstReleased:
            case Responses.IsisBaroReleased:
                OnSwitchPositionChanged(Buffer.Dequeue() - (byte)Responses.BrkFanReleased, false);
                return true;

            default:
                Buffer.TryDequeue(out byte _);
                return true;
        }
    }

    private static readonly Dictionary<char, byte> DisplayDigitCodes = new()
    {
        { ' ', 0x20 },
        { '0', 0x30 },
        { '1', 0x31 },
        { '2', 0x32 },
        { '3', 0x33 },
        { '4', 0x34 },
        { '5', 0x35 },
        { '6', 0x36 },
        { '7', 0x37 },
        { '8', 0x38 },
        { '9', 0x39 },
        { ':', 0x3A }
    };

    private enum Commands : byte
    {
        SetBacklightBrightness = 0x05,
        SetAmberIndicatorsBrightness = 0x06,
        SetWhiteIndicatorsBrightness = 0x07,
        SetRedIndicatorsBrightness = 0x08,
        SetGreenIndicatorsBrightness = 0x09,
        SetBlueIndicatorsBrightness = 0x0A,
        SetMipFloodLightBrightness = 0x0B,
        SetKeyIndicators = 0x0C,
        ClearKeyIndicators = 0x0D,
        SetAccumulatorPressureIndicatorPosition = 0x0E,
        SetLeftBrakePressureIndicatorPosition = 0x0F,
        SetRightBrakePressureIndicatorPosition = 0x10,
        RequestAntiSkidSwitchPosition = 0x11,
        RequestLandingGearLeverSwitchPosition = 0x12,
        SetClockDisplayBrightness = 0x16,
        SetClockChrDisplay = 0x17,
        SetClockUtcDisplay = 0x18,
        SetClockEtDisplay = 0x19,
        RequestClockUtcSelectorPosition = 0x1A,
        RequestClockEtSelectorPosition = 0x1B,
    }

    private enum Responses : byte
    {
        BrkFanPressed = 0x00,
        AutoBrkLoPressed = 0x01,
        AutoBrkMedPressed = 0x02,
        AutoBrkMaxPressed = 0x03,
        TerrOnNdFoPressed = 0x04,
        TerrOnNdCptPressed = 0x05,
        ClockDateSetPressed = 0x06,
        ClockRstPressed = 0x07,
        ClockChrPressed = 0x08,
        IsisBugsPressed = 0x09,
        IsisLsPressed = 0x0A,
        IsisPlusPressed = 0x0B,
        IsisMinusPressed = 0x0C,
        IsisRstPressed = 0x0D,
        IsisBaroPressed = 0x0E,
        AntiSkidOnSelected = 0x0F,
        AntiSkidOffSelected = 0x10,
        LandingGearLeverUpSelected = 0x11,
        LandingGearLeverDownSelected = 0x12,
        ClockUtcGpsSelected = 0x15,
        ClockUtcIntSelected = 0x16,
        ClockUtcSetSelected = 0x17,
        ClockEtRunSelected = 0x18,
        ClockEtStpSelected = 0x19,
        ClockEtRstSelected = 0x1A,
        ClockDateSetEncoderIncremented = 0x1B,
        ClockDateSetEncoderDecremented = 0x1C,
        IsisBaroEncoderIncremented = 0x1D,
        IsisBaroEncoderDecremented = 0x1E,
        BrkFanReleased = 0x40,
        AutoBrkLoReleased = 0x41,
        AutoBrkMedReleased = 0x42,
        AutoBrkMaxReleased = 0x43,
        TerrOnNdFoReleased = 0x44,
        TerrOnNdCptReleased = 0x45,
        ClockDateSetReleased = 0x46,
        ClockRstReleased = 0x47,
        ClockChrReleased = 0x48,
        IsisBugsReleased = 0x49,
        IsisLsReleased = 0x4A,
        IsisPlusReleased = 0x4B,
        IsisMinusReleased = 0x4C,
        IsisRstReleased = 0x4D,
        IsisBaroReleased = 0x4E
    }
}