using System;

namespace DeviceInterfaceManager.Models.FlightSim.MSFS.PMDG.SDK;

// Audio control panel selected receiver flags.
// The COMM_ReceiverSwitches[3] variables may contain any combination of these flags.
[Flags]
public enum AcpSelRecv : uint
{
    Vhf1 = 0x0001,
    Vhf2 = 0x0002,
    Vhf3 = 0x0004,
    Hf1 = 0x0008,
    Hf2 = 0x0010,
    Flt = 0x0020,
    Svc = 0x0040,
    Pa = 0x0080,
    Nav1 = 0x0100,
    Nav2 = 0x0200,
    Adf1 = 0x0400,
    Adf2 = 0x0800,
    Mkr = 0x1000,
    Spkr = 0x2000
}