using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.Services.Devices.FsCockpit;

public class FsCockpitAirbusSidestick : FsCockpitServiceBase
{
    public FsCockpitAirbusSidestick(FsCockpitSerialPortService fsCockpitSerialPortService) : base(fsCockpitSerialPortService)
    {
        Outputs.Builder outputsBuilder = new();
        Outputs = outputsBuilder.SetDatalineInfo(1, 1).Build();
    }

    protected override void StartupRequest()
    {
    }

    protected override void SetLed(byte byte1, byte byte2, byte byte3, byte byte4)
    {
    }

    protected override void ClearLed(byte byte1, byte byte2, byte byte3, byte byte4)
    {
    }

    protected override void SetDataline(byte byte1, byte byte2, byte byte3, byte byte4)
    {
        SendCommand((byte)Commands.ArmCenterPositionSolenoid);
    }

    protected override void ClearDataline(byte byte1, byte byte2, byte byte3, byte byte4)
    {
        SendCommand((byte)Commands.DisarmCenterPositionSolenoid);
    }

    protected override void SetDisplay(byte position, string data)
    {
    }

    protected override void SetAnalog(byte position, byte value)
    {
    }

    protected override bool GetValue(byte responses)
    {
        Buffer.TryDequeue(out byte _);
        return true;
    }

    private enum Commands : byte
    {
        ArmCenterPositionSolenoid = 0x06,
        DisarmCenterPositionSolenoid = 0x07,
    }
}