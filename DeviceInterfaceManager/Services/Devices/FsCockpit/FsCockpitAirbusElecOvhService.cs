using System.Collections.Generic;
using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.Services.Devices.FsCockpit;

public class FsCockpitAirbusElecOvhService : FsCockpitServiceBase
{
    public FsCockpitAirbusElecOvhService(FsCockpitSerialPortService fsCockpitSerialPortService) : base(fsCockpitSerialPortService)
    {
        Inputs.Builder inputsBuilder = new();
        Outputs.Builder outputsBuilder = new();
        Inputs = inputsBuilder.SetSwitchInfo(0, 10).Build();
        Outputs = outputsBuilder.SetLedInfo(0, 18).SetDatalineInfo(0, 1).SetSevenSegmentInfo(12, 13).SetAnalogInfo(5, 11).Build();
    }

    protected override void StartupRequest()
    {
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
        SendCommand((byte)Commands.SetIndicators, byte1);
    }

    protected override void ClearDataline(byte byte1, byte byte2, byte byte3, byte byte4)
    {
        SendCommand((byte)Commands.ClearIndicators, byte1);
    }

    protected override void SetDisplay(byte position, string data)
    {
        byte[] dataArray = GetDisplayData(DisplayDigitCodes, data, out byte decimalDigit);

        if ((Commands)position is
            Commands.SetBat1Display or
            Commands.SetBat2Display)
        {
            SendCommand(position, dataArray[0], dataArray[1], dataArray[2], decimalDigit);
        }
    }

    protected override void SetAnalog(byte position, byte value)
    {
        if ((Commands)position is
            Commands.SetBacklightBrightness or
            Commands.SetDisplayBrightness or
            Commands.SetVoltageIndicatorsBrightness or
            Commands.SetAmberIndicatorsBrightness or
            Commands.SetWhiteIndicatorsBrightness or
            Commands.SetGreenIndicatorsBrightness or
            Commands.SetBlueIndicatorsBrightness)
        {
            SendCommand(position, value);
        }
    }

    protected override bool GetValue(byte responses)
    {
        switch ((Responses)responses)
        {
            case Responses.Bat1Pressed:
            case Responses.Bat2Pressed:
            case Responses.Gen1Pressed:
            case Responses.Gen2Pressed:
            case Responses.Idg1Pressed:
            case Responses.Idg2Pressed:
            case Responses.AcEssFeedPressed:
            case Responses.GalleyPressed:
            case Responses.ApuGenPressed:
            case Responses.BusTiePressed:
            case Responses.ExtPwrPressed:
                OnSwitchPositionChanged(Buffer.Dequeue(), true);
                return true;

            case Responses.Bat1Released:
            case Responses.Bat2Released:
            case Responses.Gen1Released:
            case Responses.Gen2Released:
            case Responses.Idg1Released:
            case Responses.Idg2Released:
            case Responses.AcEssFeedReleased:
            case Responses.GalleyReleased:
            case Responses.ApuGenReleased:
            case Responses.BusTieReleased:
            case Responses.ExtPwrReleased:
                OnSwitchPositionChanged(Buffer.Dequeue() - (byte)Responses.Bat1Released, false);
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
        { ' ', 0x10 },
        { '-', 0x11 }
    };

    private enum Commands : byte
    {
        SetBacklightBrightness = 0x05,
        SetDisplayBrightness = 0x06,
        SetVoltageIndicatorsBrightness = 0x07,
        SetAmberIndicatorsBrightness = 0x08,
        SetWhiteIndicatorsBrightness = 0x09,
        SetGreenIndicatorsBrightness = 0x0A,
        SetBlueIndicatorsBrightness = 0x0B,
        SetBat1Display = 0x0C,
        SetBat2Display = 0x0D,
        SetIndicators = 0x0E,
        ClearIndicators = 0x0F,
        SetKeyIndicators = 0x10,
        ClearKeyIndicators = 0x11
    }

    private enum Responses : byte
    {
        Bat1Pressed = 0x00,
        Bat2Pressed = 0x01,
        Gen1Pressed = 0x02,
        Gen2Pressed = 0x03,
        Idg1Pressed = 0x04,
        Idg2Pressed = 0x05,
        AcEssFeedPressed = 0x06,
        GalleyPressed = 0x07,
        ApuGenPressed = 0x08,
        BusTiePressed = 0x09,
        ExtPwrPressed = 0x0A,
        Bat1Released = 0x10,
        Bat2Released = 0x11,
        Gen1Released = 0x12,
        Gen2Released = 0x13,
        Idg1Released = 0x14,
        Idg2Released = 0x15,
        AcEssFeedReleased = 0x16,
        GalleyReleased = 0x17,
        ApuGenReleased = 0x18,
        BusTieReleased = 0x19,
        ExtPwrReleased = 0x1A
    }
}