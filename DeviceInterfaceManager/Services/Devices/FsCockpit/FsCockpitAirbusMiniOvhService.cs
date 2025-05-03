using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.Services.Devices.FsCockpit;

public class FsCockpitAirbusMiniOvhService : FsCockpitServiceBase
{
    public FsCockpitAirbusMiniOvhService(FsCockpitSerialPortService fsCockpitSerialPortService) : base(fsCockpitSerialPortService)
    {
        Inputs.Builder inputsBuilder = new();
        Outputs.Builder outputsBuilder = new();
        Inputs = inputsBuilder.SetSwitchInfo(0, 50).SetAnalogInfo(51, 52).Build();
        Outputs = outputsBuilder.SetLedInfo(0, 17).SetAnalogInfo(5, 9).Build();
    }

    protected override void StartupRequest()
    {
        SendCommand((byte)Commands.RequestLdgElevPotentiometerValue);
        SendCommand((byte)Commands.RequestOvhdIntegLtPotentiometerValue);
        SendCommand((byte)Commands.RequestStrobeSwitchPosition);
        SendCommand((byte)Commands.RequestBeaconSwitchPosition);
        SendCommand((byte)Commands.RequestWingSwitchPosition);
        SendCommand((byte)Commands.RequestNavLogoSwitchPosition);
        SendCommand((byte)Commands.RequestRwyTurnOffSwitchPosition);
        SendCommand((byte)Commands.RequestLandLSwitchPosition);
        SendCommand((byte)Commands.RequestLandRSwitchPosition);
        SendCommand((byte)Commands.RequestNoseSwitchPosition);
        SendCommand((byte)Commands.RequestOvhdIntegLtSwitchPosition);
        SendCommand((byte)Commands.RequestIceIndSwitchPosition);
        SendCommand((byte)Commands.RequestDomeSwitchPosition);
        SendCommand((byte)Commands.RequestAnnLtSwitchPosition);
        SendCommand((byte)Commands.RequestSeatBeltsSwitchPosition);
        SendCommand((byte)Commands.RequestNoSmokingSwitchPosition);
        SendCommand((byte)Commands.RequestEmerExitLtSwitchPosition);
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
    }

    protected override void SetAnalog(byte position, byte value)
    {
        if ((Commands)position is
            Commands.SetBacklightBrightness or
            Commands.SetRedIndicatorsBrightness or
            Commands.SetGreenIndicatorsBrightness or
            Commands.SetBlueIndicatorsBrightness or
            Commands.SetWhiteIndicatorsBrightness)
        {
            SendCommand(position, value);
        }
    }

    protected override bool GetValue(byte responses)
    {
        switch ((Responses)responses)
        {
            case Responses.WingPressed:
            case Responses.EngOnePressed:
            case Responses.EngTwoPressed:
            case Responses.HeatPressed:
            case Responses.ApuMasterPressed:
            case Responses.ApuStartPressed:
            case Responses.ModeSelPressed:
            case Responses.DitchingPressed:
            case Responses.EmerExitPressed:
            case Responses.ManVsCtlUpPressed:
            case Responses.ManVsCtlDownPressed:
                OnSwitchPositionChanged(Buffer.Dequeue(), true);
                return true;

            case Responses.StrobeOnSelected:
            case Responses.StrobeAutoSelected:
            case Responses.StrobeOffSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.StrobeOnSelected, (byte)Responses.StrobeOffSelected);

            case Responses.RwyTurnOffOnSelected:
            case Responses.RwyTurnOffOffSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.RwyTurnOffOnSelected, (byte)Responses.RwyTurnOffOffSelected);

            case Responses.BeaconOnSelected:
            case Responses.BeaconOffSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.BeaconOnSelected, (byte)Responses.BeaconOffSelected);

            case Responses.WingOnSelected:
            case Responses.WingOffSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.WingOnSelected, (byte)Responses.WingOffSelected);

            case Responses.LandLOnSelected:
            case Responses.LandLOffSelected:
            case Responses.LandLRetractSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.LandLOnSelected, (byte)Responses.LandLRetractSelected);

            case Responses.LandROnSelected:
            case Responses.LandROffSelected:
            case Responses.LandRRetractSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.LandROnSelected, (byte)Responses.LandRRetractSelected);

            case Responses.NavLogoTwoSelected:
            case Responses.NavLogoOneSelected:
            case Responses.NavLogoOffSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.NavLogoTwoSelected, (byte)Responses.NavLogoOffSelected);

            case Responses.NoseToSelected:
            case Responses.NoseTaxiSelected:
            case Responses.NoseOffSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.NoseToSelected, (byte)Responses.NoseOffSelected);

            case Responses.OvhdIntegLtOffSelected:
            case Responses.OvhdIntegLtOnSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.OvhdIntegLtOffSelected, (byte)Responses.OvhdIntegLtOnSelected);

            case Responses.IceIndStbySelected:
            case Responses.IceIndOffSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.IceIndStbySelected, (byte)Responses.IceIndOffSelected);

            case Responses.DomeBrtSelected:
            case Responses.DomeDimSelected:
            case Responses.DomeOffSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.DomeBrtSelected, (byte)Responses.DomeOffSelected);

            case Responses.AnnLtTestSelected:
            case Responses.AnnLtBrtSelected:
            case Responses.AnnLtDimSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.AnnLtTestSelected, (byte)Responses.AnnLtDimSelected);

            case Responses.SeatBeltsOnSelected:
            case Responses.SeatBeltsAutoSelected:
            case Responses.SeatBeltsOffSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.SeatBeltsOnSelected, (byte)Responses.SeatBeltsOffSelected);

            case Responses.NoSmokingOnSelected:
            case Responses.NoSmokingAutoSelected:
            case Responses.NoSmokingOffSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.NoSmokingOnSelected, (byte)Responses.NoSmokingOffSelected);

            case Responses.EmerExitLtOnSelected:
            case Responses.EmerExitLtArmSelected:
            case Responses.EmerExitLtOffSelected:
                return HandleSwitchPositionChange(Buffer.Dequeue(), (byte)Responses.EmerExitLtOnSelected, (byte)Responses.EmerExitLtOffSelected);

            case Responses.LdgElevPotentiometerValue when Buffer.Count >= 2:
                return ProcessAnalogValueNormal((byte)Responses.LdgElevPotentiometerValue, GetFrame(2));

            case Responses.LdgElevPotentiometerValue:
                return false;

            case Responses.OvhdIntegLtPotentiometerValue when Buffer.Count >= 2:
                return ProcessAnalogValueNormal((byte)Responses.OvhdIntegLtPotentiometerValue, GetFrame(2));

            case Responses.OvhdIntegLtPotentiometerValue:
                return false;

            case Responses.WingReleased:
            case Responses.EngOneReleased:
            case Responses.EngTwoReleased:
            case Responses.HeatReleased:
            case Responses.ApuMasterReleased:
            case Responses.ApuStartReleased:
            case Responses.ModeSelReleased:
            case Responses.DitchingReleased:
            case Responses.EmerExitReleased:
            case Responses.ManVsCtlUpReleased:
            case Responses.ManVsCtlDownReleased:
                OnSwitchPositionChanged(Buffer.Dequeue() - (byte)Responses.WingReleased, false);
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
        SetGreenIndicatorsBrightness = 0x07,
        SetBlueIndicatorsBrightness = 0x08,
        SetWhiteIndicatorsBrightness = 0x09,
        SetKeyIndicators = 0x0A,
        ClearKeyIndicators = 0x0B,
        RequestLdgElevPotentiometerValue = 0x0C,
        RequestOvhdIntegLtPotentiometerValue = 0x0D,
        RequestStrobeSwitchPosition = 0x0E,
        RequestBeaconSwitchPosition = 0x0F,
        RequestWingSwitchPosition = 0x10,
        RequestNavLogoSwitchPosition = 0x11,
        RequestRwyTurnOffSwitchPosition = 0x12,
        RequestLandLSwitchPosition = 0x13,
        RequestLandRSwitchPosition = 0x14,
        RequestNoseSwitchPosition = 0x15,
        RequestOvhdIntegLtSwitchPosition = 0x16,
        RequestIceIndSwitchPosition = 0x17,
        RequestDomeSwitchPosition = 0x18,
        RequestAnnLtSwitchPosition = 0x19,
        RequestSeatBeltsSwitchPosition = 0x1A,
        RequestNoSmokingSwitchPosition = 0x1B,
        RequestEmerExitLtSwitchPosition = 0x1C
    }

    private enum Responses : byte
    {
        WingPressed = 0x00,
        EngOnePressed = 0x01,
        EngTwoPressed = 0x02,
        HeatPressed = 0x03,
        ApuMasterPressed = 0x04,
        ApuStartPressed = 0x05,
        ModeSelPressed = 0x06,
        DitchingPressed = 0x07,
        EmerExitPressed = 0x08,
        ManVsCtlUpPressed = 0x09,
        ManVsCtlDownPressed = 0x0A,
        StrobeOnSelected = 0x0B,
        StrobeAutoSelected = 0x0C,
        StrobeOffSelected = 0x0D,
        RwyTurnOffOnSelected = 0x0E,
        RwyTurnOffOffSelected = 0x0F,
        BeaconOnSelected = 0x10,
        BeaconOffSelected = 0x11,
        WingOnSelected = 0x12,
        WingOffSelected = 0x13,
        LandLOnSelected = 0x14,
        LandLOffSelected = 0x15,
        LandLRetractSelected = 0x16,
        LandROnSelected = 0x17,
        LandROffSelected = 0x18,
        LandRRetractSelected = 0x19,
        NavLogoTwoSelected = 0x1A,
        NavLogoOneSelected = 0x1B,
        NavLogoOffSelected = 0x1C,
        NoseToSelected = 0x1D,
        NoseTaxiSelected = 0x1E,
        NoseOffSelected = 0x1F,
        OvhdIntegLtOffSelected = 0x20,
        OvhdIntegLtOnSelected = 0x21,
        IceIndStbySelected = 0x22,
        IceIndOffSelected = 0x23,
        DomeBrtSelected = 0x24,
        DomeDimSelected = 0x25,
        DomeOffSelected = 0x26,
        AnnLtTestSelected = 0x27,
        AnnLtBrtSelected = 0x28,
        AnnLtDimSelected = 0x29,
        SeatBeltsOnSelected = 0x2A,
        SeatBeltsAutoSelected = 0x2B,
        SeatBeltsOffSelected = 0x2C,
        NoSmokingOnSelected = 0x2D,
        NoSmokingAutoSelected = 0x2E,
        NoSmokingOffSelected = 0x2F,
        EmerExitLtOnSelected = 0x30,
        EmerExitLtArmSelected = 0x31,
        EmerExitLtOffSelected = 0x32,
        LdgElevPotentiometerValue = 0x33,
        OvhdIntegLtPotentiometerValue = 0x34,
        WingReleased = 0x40,
        EngOneReleased = 0x41,
        EngTwoReleased = 0x42,
        HeatReleased = 0x43,
        ApuMasterReleased = 0x44,
        ApuStartReleased = 0x45,
        ModeSelReleased = 0x46,
        DitchingReleased = 0x47,
        EmerExitReleased = 0x48,
        ManVsCtlUpReleased = 0x49,
        ManVsCtlDownReleased = 0x4A,
    }
}