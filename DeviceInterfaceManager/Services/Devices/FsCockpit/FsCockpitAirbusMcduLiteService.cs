using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.Services.Devices.FsCockpit;

public class FsCockpitAirbusMcduLiteService : FsCockpitServiceBase
{
    public FsCockpitAirbusMcduLiteService(FsCockpitSerialPortService fsCockpitSerialPortService) : base(fsCockpitSerialPortService)
    {
        Inputs.Builder inputsBuilder = new();
        Outputs.Builder outputsBuilder = new();
        Inputs = inputsBuilder.SetSwitchInfo(0, 70).Build();
        Outputs = outputsBuilder.SetLedInfo(0, 6).SetAnalogInfo(5, 8).Build();
    }

    protected override void StartupRequest()
    {
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
    }

    protected override void SetAnalog(byte position, byte value)
    {
        if ((Commands)position is
            Commands.SetBacklightBrightness or
            Commands.SetAmberIndicatorsBrightness or
            Commands.SetGreenIndicatorsBrightness or
            Commands.SetWhiteIndicatorsBrightness)
        {
            SendCommand(position, value);
        }
    }

    protected override bool GetValue(byte responses)
    {
        switch ((Responses)responses)
        {
            case Responses.KeyPressed when Buffer.Count >= 2:
                Buffer.Dequeue();
                OnSwitchPositionChanged(Buffer.Dequeue(), true);
                return true;

            case Responses.KeyPressed:
                return false;

            case Responses.KeyReleased when Buffer.Count >= 2:
                Buffer.Dequeue();
                OnSwitchPositionChanged(Buffer.Dequeue(), false);
                return true;

            case Responses.KeyReleased:
                return false;

            default:
                Buffer.TryDequeue(out byte _);
                return true;
        }
    }

    /*
    private enum KeyCodes : byte
    {
        Zero = 0x00,
        One = 0x01,
        Two = 0x02,
        Three = 0x03,
        Four = 0x04,
        Five = 0x05,
        Six = 0x06,
        Seven = 0x07,
        Eight = 0x08,
        Nine = 0x09,
        A = 0x0A,
        B = 0x0B,
        C = 0x0C,
        D = 0x0D,
        E = 0x0E,
        F = 0x0F,
        G = 0x10,
        H = 0x11,
        I = 0x12,
        J = 0x13,
        K = 0x14,
        L = 0x15,
        M = 0x16,
        N = 0x17,
        O = 0x18,
        P = 0x19,
        Q = 0x1A,
        R = 0x1B,
        S = 0x1C,
        T = 0x1D,
        U = 0x1E,
        V = 0x1F,
        W = 0x20,
        X = 0x21,
        Y = 0x22,
        Z = 0x23,
        OneL = 0x24,
        TwoL = 0x25,
        ThreeL = 0x26,
        FourL = 0x27,
        FiveL = 0x28,
        SixL = 0x29,
        OneR = 0x2A,
        TwoR = 0x2B,
        ThreeR = 0x2C,
        FourR = 0x2D,
        FiveR = 0x2E,
        SixR = 0x2F,
        Dot = 0x30,
        Minus = 0x31,
        Slash = 0x32,
        Space = 0x33,
        Ovfly = 0x34,
        Clear = 0x35,
        Dir = 0x36,
        Prog = 0x37,
        Perf = 0x38,
        Init = 0x39,
        Data = 0x3A,
        FPln = 0x3B,
        RadNav = 0x3C,
        FuelPred = 0x3D,
        SecFPln = 0x3E,
        AtcCom = 0x3F,
        Menu = 0x40,
        Airport = 0x41,
        Dummy = 0x42,
        Left = 0x43,
        Right = 0x44,
        Up = 0x45,
        Down = 0x46
    }
    */

    private enum Commands : byte
    {
        SetBacklightBrightness = 0x05,
        SetAmberIndicatorsBrightness = 0x06,
        SetGreenIndicatorsBrightness = 0x07,
        SetWhiteIndicatorsBrightness = 0x08,
        SetIndicators = 0x09,
        ClearIndicators = 0x0A
    }

    private enum Responses : byte
    {
        KeyPressed = 0x00,
        KeyReleased = 0x01
    }
}