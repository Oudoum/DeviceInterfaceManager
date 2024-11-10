namespace DeviceInterfaceManager.Models.FlightSim.MSFS.PMDG;

public enum Mouse : uint
{
    RightSingle = 0x80000000u,
    // MiddleSingle = 0x40000000u,
    LeftSingle = 0x20000000u,
    // RightDouble = 0x10000000u,
    // MiddleDouble = 0x08000000u,
    // LeftDouble = 0x04000000u,
    // RightDrag = 0x02000000u,
    // MiddleDrag = 0x01000000u,
    // LeftDrag = 0x00800000u,
    // Move = 0x00400000u,
    // DownRepeat = 0x00200000u,
    RightRelease = 0x00080000u,
    // MiddleRelease = 0x00040000u,
    LeftRelease = 0x00020000u,
    // WheelFlip = 0x00010000u, // invert direction of mouse wheel
    // WheelSkip = 0x00008000u, // look at next 2 rect for mouse wheel commands
    WheelUp = 0x00004000u,
    WheelDown = 0x00002000u
}