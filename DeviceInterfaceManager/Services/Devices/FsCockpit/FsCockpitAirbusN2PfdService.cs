using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.Services.Devices.FsCockpit;

public class FsCockpitAirbusN2PfdService : FsCockpitServiceBase
{
    public FsCockpitAirbusN2PfdService(FsCockpitSerialPortService fsCockpitSerialPortService) : base(fsCockpitSerialPortService)
    {
        Inputs.Builder inputsBuilder = new();
        Outputs.Builder outputsBuilder = new();
        Inputs = inputsBuilder.SetSwitchInfo(0, 7).SetAnalogInfo(13, 16).Build();
        Outputs = outputsBuilder.SetLedInfo(0, 1).SetAnalogInfo(5, 7).Build();
    }

    protected override void StartupRequest()
    {
        SendCommand((byte)Commands.RequestPfdPotentiometerSwitch);
        SendCommand((byte)Commands.RequestNdPotentiometerSwitch);
        SendCommand((byte)Commands.RequestLoudspeakerPotentiometerSwitch);
        SendCommand((byte)Commands.RequestPfdPotentiometerValue);
        SendCommand((byte)Commands.RequestNdInnerPotentiometerValue);
        SendCommand((byte)Commands.RequestNdOuterPotentiometerValue);
        SendCommand((byte)Commands.RequestLoudspeakerPotentiometerValue);
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
    }

    protected override void SetAnalog(byte position, byte value)
    {
        if ((Commands)position is
            Commands.SetBacklightBrightness or
            Commands.SetRedIndicatorsBrightness or
            Commands.SetAmberIndicatorsBrightness)
        {
            SendCommand(position, value);
        }
    }

    protected override bool GetValue(byte responses)
    {
        switch ((Responses)responses)
        {
            case Responses.GpwsPressed:
            case Responses.PfdNdXfrPressed:
                OnSwitchPositionChanged(Buffer.Dequeue(), true);
                return true;

            case Responses.PfdOffSelected:
            case Responses.PfdOnSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.PfdOffSelected, (byte)Responses.PfdOnSelected);

            case Responses.NdOffSelected:
            case Responses.NdOnSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.NdOffSelected, (byte)Responses.NdOnSelected);

            case Responses.LoudspeakerOffSelected:
            case Responses.LoudspeakerOnSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.LoudspeakerOffSelected, (byte)Responses.LoudspeakerOnSelected);

            case Responses.PfdPotentiometerValue when Buffer.Count >= 2:
                return ProcessAnalogValueNormal((byte)Responses.PfdPotentiometerValue, GetFrame(2));

            case Responses.PfdPotentiometerValue:
                return false;

            case Responses.NdInnerPotentiometerValue when Buffer.Count >= 2:
                return ProcessAnalogValueNormal((byte)Responses.NdInnerPotentiometerValue, GetFrame(2));

            case Responses.NdInnerPotentiometerValue:
                return false;

            case Responses.NdOuterPotentiometerValue when Buffer.Count >= 2:
                return ProcessAnalogValueNormal((byte)Responses.NdOuterPotentiometerValue, GetFrame(2));

            case Responses.NdOuterPotentiometerValue:
                return false;

            case Responses.LoudspeakerPotentiometerValue when Buffer.Count >= 2:
                return ProcessAnalogValueNormal((byte)Responses.LoudspeakerPotentiometerValue, GetFrame(2));

            case Responses.LoudspeakerPotentiometerValue:
                return false;

            case Responses.GpwsReleased:
            case Responses.PfdNdXfrReleased:
                OnSwitchPositionChanged(Buffer.Dequeue() - (byte)Responses.GpwsReleased, false);
                return true;

            default:
                Buffer.TryDequeue(out byte _);
                return true;
        }
    }

    private enum Commands : byte
    {
        SetBacklightBrightness = 0x05,
        SetRedIndicatorsBrightness = 0x06,
        SetAmberIndicatorsBrightness = 0x07,
        SetKeyIndicators = 0x08,
        ClearKeyIndicators = 0x09,
        RequestPfdPotentiometerSwitch = 0x0C,
        RequestNdPotentiometerSwitch = 0x0D,
        RequestLoudspeakerPotentiometerSwitch = 0x0E,
        RequestPfdPotentiometerValue = 0x0F,
        RequestNdInnerPotentiometerValue = 0x10,
        RequestNdOuterPotentiometerValue = 0x11,
        RequestLoudspeakerPotentiometerValue = 0x12
    }

    private enum Responses : byte
    {
        GpwsPressed = 0x00,
        PfdNdXfrPressed = 0x01,
        PfdOffSelected = 0x02,
        PfdOnSelected = 0x03,
        NdOffSelected = 0x04,
        NdOnSelected = 0x05,
        LoudspeakerOffSelected = 0x06,
        LoudspeakerOnSelected = 0x07,
        PfdPotentiometerValue = 0x0D,
        NdInnerPotentiometerValue = 0x0E,
        NdOuterPotentiometerValue = 0x0F,
        LoudspeakerPotentiometerValue = 0x10,
        GpwsReleased = 0x20,
        PfdNdXfrReleased = 0x21,
    }
}