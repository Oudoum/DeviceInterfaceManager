namespace DeviceInterfaceManager.Models.FlightSim.MSFS.PMDG;

public enum Mouse : long
{
    RightSingle = 0x80000000,
    // MiddleSingle = 0x40000000,
    LeftSingle = 0x20000000,
    // RightDouble = 0x10000000,
    // MiddleDouble = 0x08000000,
    // LeftDouble = 0x04000000,
    // RightDrag = 0x02000000,
    // MiddleDrag = 0x01000000,
    // LeftDrag = 0x00800000,
    // Move = 0x00400000,
    // DownRepeat = 0x00200000,
    RightRelease = 0x00080000,
    // MiddleRelease = 0x00040000,
    LeftRelease = 0x00020000,
    // WheelFlip = 0x00010000, // invert direction of mouse wheel
    // WheelSkip = 0x00008000, // look at next 2 rect for mouse wheel commands
    WheelUp = 0x00004000,
    WheelDown = 0x00002000
}