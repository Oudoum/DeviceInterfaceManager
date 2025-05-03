using System.Collections.Generic;
using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.Services.Devices.FsCockpit;

public class FsCockpitAirbusFcuLiteService : FsCockpitServiceBase
{
    private bool _skipNextEncoderFrame;

    public FsCockpitAirbusFcuLiteService(FsCockpitSerialPortService fsCockpitSerialPortService) : base(fsCockpitSerialPortService)
    {
        Inputs.Builder inputsBuilder = new();
        Outputs.Builder outputsBuilder = new();
        Inputs = inputsBuilder.SetSwitchInfo(0, 26).Build();
        Outputs = outputsBuilder.SetLedInfo(1, 6).SetDatalineInfo(0, 14).SetSevenSegmentInfo(10, 13).SetAnalogInfo(5, 9).Build();
    }

    protected override void StartupRequest()
    {
        SendCommand((byte)Commands.RequestSelectorPosition);
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
        SendCommand((byte)Commands.SetIndicators, byte1, byte2);
    }

    protected override void ClearDataline(byte byte1, byte byte2, byte byte3, byte byte4)
    {
        SendCommand((byte)Commands.ClearIndicators, byte1, byte2);
    }

    protected override void SetDisplay(byte position, string data)
    {
        byte[] dataArray = GetDisplayData(DisplayDigitCodes, data, out byte decimalDigit);

        switch ((Commands)position)
        {
            case Commands.SetSpdDisplay:
            case Commands.SetHdgDisplay:
                SendCommand(position, dataArray[0], dataArray[1], dataArray[2], decimalDigit);
                break;

            case Commands.SetAltDisplay:
            case Commands.SetVsDisplay:
                SendCommand(position, dataArray[0], dataArray[1], dataArray[2], dataArray[3], dataArray[4], decimalDigit);
                break;
        }
    }

    protected override void SetAnalog(byte position, byte value)
    {
        if ((Commands)position is
            Commands.SetBacklightBrightness or
            Commands.SetKeyBacklightBrightness or
            Commands.SetKeyIndicatorsBrightness or
            Commands.SetIndicatorsBrightness or
            Commands.SetDisplayBrightness)
        {
            SendCommand(position, value);
        }
    }

    protected override bool GetValue(byte responses)
    {
        switch ((Responses)responses)
        {
            case Responses.Ap1Pressed:
            case Responses.Ap2Pressed:
            case Responses.LocPressed:
            case Responses.AthrPressed:
            case Responses.ExpedPressed:
            case Responses.ApprPressed:
            case Responses.SpdMachPressed:
            case Responses.HdgTrkPressed:
            case Responses.MetricAltPressed:
            case Responses.Push0Pressed:
            case Responses.Pull0Pressed:
            case Responses.Push1Pressed:
            case Responses.Pull1Pressed:
            case Responses.Push2Pressed:
            case Responses.Pull2Pressed:
            case Responses.Push3Pressed:
            case Responses.Pull3Pressed:
                OnSwitchPositionChanged(Buffer.Dequeue(), true);
                return true;

            case Responses.HundredSelected:
            case Responses.ThousandSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.HundredSelected, (byte)Responses.ThousandSelected);

            case Responses.Encoder0Incremented:
            case Responses.Encoder0Decremented:
            case Responses.Encoder1Incremented:
            case Responses.Encoder1Decremented:
            case Responses.Encoder2Incremented:
            case Responses.Encoder2Decremented:
            case Responses.Encoder3Incremented:
            case Responses.Encoder3Decremented:
                return HandleSwitchPositionChange(Buffer.Dequeue(), ref _skipNextEncoderFrame);

            case Responses.Ap1Released:
            case Responses.Ap2Released:
            case Responses.LocReleased:
            case Responses.AthrReleased:
            case Responses.ExpedReleased:
            case Responses.ApprReleased:
            case Responses.SpdMachReleased:
            case Responses.HdgTrkReleased:
            case Responses.MetricAltReleased:
            case Responses.Push0Released:
            case Responses.Pull0Released:
            case Responses.Push1Released:
            case Responses.Pull1Released:
            case Responses.Push2Released:
            case Responses.Pull2Released:
            case Responses.Push3Released:
            case Responses.Pull3Released:
                OnSwitchPositionChanged(Buffer.Dequeue() - (byte)Responses.Ap1Released, false);
                return true;

            default:
                Buffer.TryDequeue(out byte _);
                return true;
        }
    }

    public static readonly Dictionary<char, byte> DisplayDigitCodes = new()
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
        { 'A', 0x0a },
        { 'b', 0x0b },
        { 'c', 0x0c },
        { 'd', 0x0d },
        { 'E', 0x0e },
        { 'F', 0x0f },
        { ' ', 0x10 },
        { '-', 0x11 },
        { 'o', 0x12 },
        { 't', 0x13 },
        { 'r', 0x14 },
        { 'q', 0x15 },
        { 'n', 0x16 },
        { 'H', 0x17 },
    };

    private enum Commands : byte
    {
        SetBacklightBrightness = 0x05,
        SetKeyBacklightBrightness = 0x06,
        SetKeyIndicatorsBrightness = 0x07,
        SetIndicatorsBrightness = 0x08,
        SetDisplayBrightness = 0x09,
        SetSpdDisplay = 0x0A,
        SetHdgDisplay = 0x0B,
        SetAltDisplay = 0x0C,
        SetVsDisplay = 0x0D,
        SetIndicators = 0x0E,
        ClearIndicators = 0x0F,
        SetKeyIndicators = 0x10,
        ClearKeyIndicators = 0x11,
        RequestSelectorPosition = 0x12,
    }

    private enum Responses : byte
    {
        Ap1Pressed = 0x00,
        Ap2Pressed = 0x01,
        LocPressed = 0x02,
        AthrPressed = 0x03,
        ExpedPressed = 0x04,
        ApprPressed = 0x05,
        SpdMachPressed = 0x06,
        HdgTrkPressed = 0x07,
        MetricAltPressed = 0x08,
        Push0Pressed = 0x09,
        Pull0Pressed = 0x0A,
        Push1Pressed = 0x0B,
        Pull1Pressed = 0x0C,
        Push2Pressed = 0x0D,
        Pull2Pressed = 0x0E,
        Push3Pressed = 0x0F,
        Pull3Pressed = 0x10,
        HundredSelected = 0x11,
        ThousandSelected = 0x12,
        Encoder0Incremented = 0x13,
        Encoder0Decremented = 0x14,
        Encoder1Incremented = 0x15,
        Encoder1Decremented = 0x16,
        Encoder2Incremented = 0x17,
        Encoder2Decremented = 0x18,
        Encoder3Incremented = 0x19,
        Encoder3Decremented = 0x1A,
        Ap1Released = 0x40,
        Ap2Released = 0x41,
        LocReleased = 0x42,
        AthrReleased = 0x43,
        ExpedReleased = 0x44,
        ApprReleased = 0x45,
        SpdMachReleased = 0x46,
        HdgTrkReleased = 0x47,
        MetricAltReleased = 0x48,
        Push0Released = 0x49,
        Pull0Released = 0x4A,
        Push1Released = 0x4B,
        Pull1Released = 0x4C,
        Push2Released = 0x4D,
        Pull2Released = 0x4E,
        Push3Released = 0x4F,
        Pull3Released = 0x50,
    }
}