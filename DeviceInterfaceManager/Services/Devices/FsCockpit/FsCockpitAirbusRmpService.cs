using System.Collections.Generic;
using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.Services.Devices.FsCockpit;

public class FsCockpitAirbusRmpService : FsCockpitServiceBase
{
    public FsCockpitAirbusRmpService(FsCockpitSerialPortService fsCockpitSerialPortService) : base(fsCockpitSerialPortService)
    {
        Inputs.Builder inputsBuilder = new();
        Outputs.Builder outputsBuilder = new();
        Inputs = inputsBuilder.SetSwitchInfo(0, 12).Build();
        Outputs = outputsBuilder.SetLedInfo(0, 6).SetSevenSegmentInfo(8, 9).SetAnalogInfo(5, 7).Build();
    }

    protected override void StartupRequest()
    {
        SendCommand((byte)Commands.RequestOnOffSwitchPosition);
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
    }

    protected override void ClearDataline(byte byte1, byte byte2, byte byte3, byte byte4)
    {
    }

    protected override void SetDisplay(byte position, string data)
    {
        byte[] dataArray = GetDisplayData(DisplayDigitCodes, data, out byte decimalDigit);

        if ((Commands)position is
            Commands.SetActiveDisplay or
            Commands.SetStandbyDisplay)
        {
            SendCommand(position, dataArray[0], dataArray[1], dataArray[2], dataArray[3], dataArray[4], dataArray[5], decimalDigit);
        }
    }

    protected override void SetAnalog(byte position, byte value)
    {
        if ((Commands)position is
            Commands.SetBacklightBrightness or
            Commands.SetDisplayBrightness or
            Commands.SetKeyIndicatorsBrightness)
        {
            SendCommand(position, value);
        }
    }

    protected override bool GetValue(byte responses)
    {
        switch ((Responses)responses)
        {
            case Responses.TfrPressed:
            case Responses.Vhf1Pressed:
            case Responses.Vhf2Pressed:
            case Responses.NavPressed:
            case Responses.VorPressed:
            case Responses.IlsPressed:
            case Responses.AdfPressed:
                OnSwitchPositionChanged(Buffer.Dequeue(), true);
                return true;

            case Responses.OnSelected:
            case Responses.OffSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.OnSelected, (byte)Responses.OffSelected);

            case Responses.EncoderInnerDecremented:
            case Responses.EncoderInnerIncremented:
            case Responses.EncoderOuterDecremented:
            case Responses.EncoderOuterIncremented:
                return HandleSwitchPositionChange(Buffer.Dequeue());

            case Responses.TfrReleased:
            case Responses.Vhf1Released:
            case Responses.Vhf2Released:
            case Responses.NavReleased:
            case Responses.VorReleased:
            case Responses.IlsReleased:
            case Responses.AdfReleased:
                OnSwitchPositionChanged(Buffer.Dequeue() - (byte)Responses.TfrReleased, false);
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
        { '-', 0x0a },
        { 'C', 0x0b },
        { ' ', 0x0c }
    };

    private enum Commands : byte
    {
        SetBacklightBrightness = 0x05,
        SetDisplayBrightness = 0x06,
        SetKeyIndicatorsBrightness = 0x07,
        SetActiveDisplay = 0x08,
        SetStandbyDisplay = 0x09,
        SetKeyIndicators = 0x0A,
        ClearKeyIndicators = 0x0B,
        RequestOnOffSwitchPosition = 0x0C
    }

    private enum Responses : byte
    {
        TfrPressed = 0x00,
        Vhf1Pressed = 0x01,
        Vhf2Pressed = 0x02,
        NavPressed = 0x03,
        VorPressed = 0x04,
        IlsPressed = 0x05,
        AdfPressed = 0x06,
        OnSelected = 0x07,
        OffSelected = 0x08,
        EncoderInnerDecremented = 0x09,
        EncoderInnerIncremented = 0x0A,
        EncoderOuterDecremented = 0x0B,
        EncoderOuterIncremented = 0x0C,
        TfrReleased = 0x10,
        Vhf1Released = 0x11,
        Vhf2Released = 0x12,
        NavReleased = 0x13,
        VorReleased = 0x14,
        IlsReleased = 0x15,
        AdfReleased = 0x16,
    }
}