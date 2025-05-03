using System.Collections.Generic;
using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.Services.Devices.FsCockpit;

public class FsCockpitAirbusTcasLpfoService : FsCockpitServiceBase
{
    public FsCockpitAirbusTcasLpfoService(FsCockpitSerialPortService fsCockpitSerialPortService) : base(fsCockpitSerialPortService)
    {
        Inputs.Builder inputsBuilder = new();
        Outputs.Builder outputsBuilder = new();
        Inputs = inputsBuilder.SetSwitchInfo(0, 28).SetAnalogInfo(14, 14).Build();
        Outputs = outputsBuilder.SetLedInfo(9, 9).SetSevenSegmentInfo(8, 8).SetAnalogInfo(5, 7).Build();
    }

    protected override void StartupRequest()
    {
        SendCommand((byte)Commands.RequestFloodLtOnOffSwitch);
        SendCommand((byte)Commands.RequestFloodLtPotentiometerValue);
        SendCommand((byte)Commands.RequestAtcStbAutoOnSelector);
        SendCommand((byte)Commands.RequestAtcOneTwoSelector);
        SendCommand((byte)Commands.RequestAltRptgSelector);
        SendCommand((byte)Commands.RequestTcasThrtAllAbvBlwSelector);
        SendCommand((byte)Commands.RequestTcasStbyTaTaraSelector);
    }

    protected override void SetLed(byte byte1, byte byte2, byte byte3, byte byte4)
    {
        if (byte1 == (byte)Commands.SetAtcFailIndicator)
        {
            SendCommand((byte)Commands.SetAtcFailIndicator);
        }
    }

    protected override void ClearLed(byte byte1, byte byte2, byte byte3, byte byte4)
    {
        if (byte1 + 1 == (byte)Commands.ClearAtcFailIndicator)
        {
            SendCommand((byte)Commands.ClearAtcFailIndicator);
        }
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
            SendCommand(position, dataArray[0], dataArray[1], dataArray[2], dataArray[3], decimalDigit);
        }
    }

    protected override void SetAnalog(byte position, byte value)
    {
        if ((Commands)position is
            Commands.SetBacklightBrightness or
            Commands.SetDisplayBrightness or
            Commands.SetAtcFailIndicatorBrightness)
        {
            SendCommand(position, value);
        }
    }

    protected override bool GetValue(byte responses)
    {
        switch ((Responses)responses)
        {
            case Responses.ZeoPressed:
            case Responses.OnePressed:
            case Responses.TwoPressed:
            case Responses.ThreePressed:
            case Responses.FourPressed:
            case Responses.FivePressed:
            case Responses.SixPressed:
            case Responses.SevenPressed:
            case Responses.ClrPressed:
            case Responses.AidsPrintPressed:
            case Responses.DfdrEventPressed:
            case Responses.IdentPressed:
                OnSwitchPositionChanged(Buffer.Dequeue(), true);
                return true;

            case Responses.FloodLtOff:
            case Responses.FloodLtOn:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.FloodLtOff, (byte)Responses.FloodLtOn);
            
            case Responses.FloodLtPotentiometerValue when Buffer.Count >= 2:
                return ProcessAnalogValueNormal((byte)Responses.FloodLtPotentiometerValue, GetFrame(2));

            case Responses.FloodLtPotentiometerValue:
                return false;

            case Responses.AtcStbySelected:
            case Responses.AtcAutoSelected:
            case Responses.AtcOnSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.AtcStbySelected, (byte)Responses.AtcOnSelected);

            case Responses.AtcOneSelected:
            case Responses.AtcTwoSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.AtcOneSelected, (byte)Responses.AtcTwoSelected);

            case Responses.AltRptgOffSelected:
            case Responses.AltRptgOnSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.AltRptgOffSelected, (byte)Responses.AltRptgOnSelected);

            case Responses.TcasThrtSelected:
            case Responses.TcasAllSelected:
            case Responses.TcasAbvSelected:
            case Responses.TcasBlwSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.TcasThrtSelected, (byte)Responses.TcasBlwSelected);

            case Responses.TcasStbySelected:
            case Responses.TcasTaSelected:
            case Responses.TcasTaRaSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.TcasStbySelected, (byte)Responses.TcasTaRaSelected);

            case Responses.ZeoReleased:
            case Responses.OneReleased:
            case Responses.TwoReleased:
            case Responses.ThreeReleased:
            case Responses.FourReleased:
            case Responses.FiveReleased:
            case Responses.SixReleased:
            case Responses.SevenReleased:
            case Responses.ClrReleased:
            case Responses.AidsPrintReleased:
            case Responses.DfdrEventReleased:
            case Responses.IdentReleased:
                OnSwitchPositionChanged(Buffer.Dequeue() - (byte)Responses.ZeoReleased, false);
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
        { ' ', 0x09 },
        { '-', 0x0A },
    };

    private enum Commands : byte
    {
        SetBacklightBrightness = 0x05,
        SetDisplayBrightness = 0x06,
        SetAtcFailIndicatorBrightness = 0x07,
        SetDisplay = 0x08,
        SetAtcFailIndicator = 0x09,
        ClearAtcFailIndicator = 0x0A,
        RequestFloodLtOnOffSwitch = 0x0B,
        RequestFloodLtPotentiometerValue = 0x0C,
        RequestAtcStbAutoOnSelector = 0x0D,
        RequestAtcOneTwoSelector = 0x0E,
        RequestAltRptgSelector = 0x0F,
        RequestTcasThrtAllAbvBlwSelector = 0x10,
        RequestTcasStbyTaTaraSelector = 0x11,
    }

    private enum Responses : byte
    {
        ZeoPressed = 0x00,
        OnePressed = 0x01,
        TwoPressed = 0x02,
        ThreePressed = 0x03,
        FourPressed = 0x04,
        FivePressed = 0x05,
        SixPressed = 0x06,
        SevenPressed = 0x07,
        ClrPressed = 0x08,
        AidsPrintPressed = 0x09,
        DfdrEventPressed = 0x0A,
        IdentPressed = 0x0B,
        FloodLtOff = 0x0C,
        FloodLtOn = 0x0D,
        FloodLtPotentiometerValue = 0x0E,
        AtcStbySelected = 0x0F,
        AtcAutoSelected = 0x10,
        AtcOnSelected = 0x11,
        AtcOneSelected = 0x12,
        AtcTwoSelected = 0x13,
        AltRptgOffSelected = 0x14,
        AltRptgOnSelected = 0x15,
        TcasThrtSelected = 0x16,
        TcasAllSelected = 0x17,
        TcasAbvSelected = 0x18,
        TcasBlwSelected = 0x19,
        TcasStbySelected = 0x1A,
        TcasTaSelected = 0x1B,
        TcasTaRaSelected = 0x1C,
        ZeoReleased = 0x20,
        OneReleased = 0x21,
        TwoReleased = 0x22,
        ThreeReleased = 0x23,
        FourReleased = 0x24,
        FiveReleased = 0x25,
        SixReleased = 0x26,
        SevenReleased = 0x27,
        ClrReleased = 0x28,
        AidsPrintReleased = 0x29,
        DfdrEventReleased = 0x2A,
        IdentReleased = 0x2B
    }
}