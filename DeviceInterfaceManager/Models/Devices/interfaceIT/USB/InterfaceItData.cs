﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DeviceInterfaceManager.Models.Devices.interfaceIT.USB;

public partial class InterfaceItData : IDeviceSerial
{
    public ComponentInfo Switch { get; private set; }
    public event EventHandler<InputChangedEventArgs>? InputChanged;
    public ComponentInfo Led { get; private set; }
    public ComponentInfo Dataline { get; private set; }
    public ComponentInfo SevenSegment { get; private set; }

    public Task SetLedAsync(string position, bool isEnabled)
    {
        interfaceIT_LED_Set(_session, Convert.ToInt32(position), isEnabled);
        return Task.CompletedTask;
    }

    public Task SetDatalineAsync(string position, bool isEnabled)
    {
        interfaceIT_Dataline_Set(_session, Convert.ToInt32(position), isEnabled);
        return Task.CompletedTask;
    }

    public Task SetSevenSegmentAsync(string position, string data)
    {
        interfaceIT_7Segment_Display(_session, data, Convert.ToInt32(position));
        return Task.CompletedTask;
    }

    public string? DeviceName { get; private set; }
    public string? SerialNumber { get; private set; }

    public Task<ConnectionStatus> ConnectAsync()
    {
        foreach (string device in interfaceIT_GetDeviceList())
        {
            if (ErrorCodes.ControllerAlreadyBound == interfaceIT_Bind(device, ref _session))
            {
                continue;
            }

            interfaceIT_GetBoardInfo(_session, out BoardInfo boardInfo);
            InterfaceItBoardId boardId = GetInterfaceItBoardId(boardInfo.BoardType);
            if (boardId is InterfaceItBoardId.FDS_CONTROLLER_MCP or InterfaceItBoardId.FDS_737_PMX_MCP or InterfaceItBoardId.JetMAX_737_RADIO)
            {
                interfaceIT_SetBoardOptions(_session, (uint)BoardOptions.Force64);
            }

            SerialNumber = device;
            DeviceName = boardId.ToString().Replace('_', ' ') + $" ({boardInfo.BoardType})";
            _features = boardInfo.Features;
            Switch = new ComponentInfo(boardInfo.SwitchFirst, boardInfo.SwitchLast);
            Led = new ComponentInfo(boardInfo.LedFirst, boardInfo.LedLast);
            Dataline = new ComponentInfo(boardInfo.DatalineFirst, boardInfo.DatalineLast);
            SevenSegment = new ComponentInfo(boardInfo.SevenSegmentFirst, boardInfo.SevenSegmentLast);

            EnableDeviceFeatures();
            interfaceIT_Switch_Enable_Callback(_session, true, _keyNotifyCallback = KeyPressedProc);

            return Task.FromResult(ConnectionStatus.Connected);
        }

        return Task.FromResult(ConnectionStatus.NotConnected);
    }

    public void Disconnect()
    {
        interfaceIT_Switch_Enable_Callback(_session, false, _keyNotifyCallback = null);
        DisableDeviceFeatures();
        interfaceIT_UnBind(_session);
    }

    private KeyNotifyCallback? _keyNotifyCallback;

    private void KeyPressedProc(uint session, int key, uint direction)
    {
        bool isPressed = direction == (int)SwitchDirection.Down;
        Switch.UpdatePosition(key, isPressed);
        InputChanged?.Invoke(this, new InputChangedEventArgs(key, isPressed));
    }

    //
    public static void OpenControllers()
    {
        interfaceIT_OpenControllers();
    }

    public static void CloseControllers()
    {
        interfaceIT_CloseControllers();
    }

    public static int TotalControllers => interfaceIT_GetTotalControllers();

    public static string ApiVersion => interfaceIT_GetAPIVersion();

    private static bool _isLoggingEnabled;

    public static bool IsLoggingEnabled
    {
        get => _isLoggingEnabled;
        set => interfaceIT_EnableLogging(_isLoggingEnabled = value);
    }

    private Features _features;
    private uint _session;

    // API
    private delegate void KeyNotifyCallback(uint session, int key, uint direction);

    private enum SwitchDirection : byte
    {
        Unknown = 0xFF,
        Up = 0x0,
        Down = 0x1
    }

    [Flags]
    private enum BoardOptions : byte
    {
        None = 0x0,
        CduKeys = 0x1, // CDU v9 does not need this.  Used for v7 CDU
        Force64 = 0x2, // Required for the new FDS-CONTROLLER-MCP boards for relay to function
        Reserved3 = 0x4
    }

    private enum ErrorCodes : short
    {
        Ok = 0,
        ControllersOpenFailed = -1,
        ControllersAlreadyOpened = -2,
        ControllersNotOpened = -3,
        InvalidHandle = -4,
        InvalidPointer = -5,
        InvalidControllerName = -6,
        Failed = -7,
        InvalidControllerPointer = -8,
        InvalidCallback = -9,
        RetrievingController = -10,
        NotEnabled = -11,
        BufferNotLargeEnough = -12,
        ParameterLengthIncorrect = -13,
        ParameterOutOfRange = -14,
        FeatureNotAvailable = -15,
        AlreadyEnabled = -16,
        NoItems = -17,
        ControllerAlreadyBound = -18,
        NoControllersFound = -19,
        UnknownError = -20,
        NotLicensed = -21,
        InvalidLicense = -22,
        AlreadyLicensed = -23,
        GeneratingActivationIdFailed = -24,
        ExpiredLicense = -25,
        ExpiredTrial = -26
    }

    [Flags]
    private enum Features : uint
    {
        None = 0x00000000,

        InputSwitches = 0x00000001,
        InputRc = 0x00000002,
        InputSpi = 0x00000004,
        InputDataLine = 0x00000008,
        InputIic = 0x00000010,
        InputReserved6 = 0x00000020,
        InputReserved7 = 0x00000040,
        InputReserved8 = 0x00000080,

        OutputLed = 0x00000100,
        OutputLcd = 0x00000200,
        Output7Segment = 0x00000400,
        OutputSpi = 0x00000800,
        OutputIic = 0x00001000,
        OutputDataLine = 0x00002000,
        OutputServo = 0x00004000,
        OutputReserved16 = 0x00008000,

        SpecialBrightness = 0x00010000,
        SpecialAnalogInput = 0x00020000,
        SpecialAnalog16Input = 0x00040000
    }

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private readonly struct BoardInfo
    {
        public readonly int LedCount;
        public readonly int LedFirst;
        public readonly int LedLast;

        public readonly int SwitchCount;
        public readonly int SwitchFirst;
        public readonly int SwitchLast;

        public readonly int SevenSegmentCount;
        public readonly int SevenSegmentFirst;
        public readonly int SevenSegmentLast;

        public readonly int DatalineCount;
        public readonly int DatalineFirst;
        public readonly int DatalineLast;

        public readonly int ServoControllerCount;
        public readonly int ServoControllerFirst;
        public readonly int ServoControllerLast;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public readonly int[] Reserved;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
        public readonly string BoardType;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public readonly string ManufactureDate;

        public readonly Features Features;
        public readonly int UpdateLevel;
    }
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    //Main Functions
    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCodes interfaceIT_OpenControllers();

    [LibraryImport("interfaceITAPI x64.dll", StringMarshalling = StringMarshalling.Utf8)]
    private static partial ErrorCodes interfaceIT_GetDeviceList(byte[]? buffer, ref uint bufferSize, string? boardType);

    private static IEnumerable<string> interfaceIT_GetDeviceList()
    {
        uint bufferSize = 0;
        interfaceIT_GetDeviceList(null, ref bufferSize, null);
        byte[] deviceList = new byte[bufferSize];
        interfaceIT_GetDeviceList(deviceList, ref bufferSize, null);
        return Encoding.UTF8.GetString(deviceList).TrimEnd('\0').Split('\0');
    }

    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCodes interfaceIT_CloseControllers();


    //Controller Functions
    [LibraryImport("interfaceITAPI x64.dll", StringMarshalling = StringMarshalling.Utf8)]
    private static partial ErrorCodes interfaceIT_Bind(string controller, ref uint session);

    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCodes interfaceIT_UnBind(uint session);

    [DllImport("interfaceITAPI x64.dll")]
    [SuppressMessage("Interoperability", "SYSLIB1054:Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time", Justification = "<Pending>")]
    private static extern ErrorCodes interfaceIT_GetBoardInfo(uint session, out BoardInfo boardInfo);

    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCodes interfaceIT_SetBoardOptions(uint session, uint options);

    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCodes interfaceIT_GetTotalControllers(ref int controllerCount);

    private static int interfaceIT_GetTotalControllers()
    {
        int controllerCount = -1;
        interfaceIT_GetTotalControllers(ref controllerCount);
        return controllerCount;
    }


    //LED Functions
    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCodes interfaceIT_LED_Enable(uint session, [MarshalAs(UnmanagedType.Bool)] bool enable);

    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCodes interfaceIT_LED_Test(uint session, [MarshalAs(UnmanagedType.Bool)] bool enable);

    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCodes interfaceIT_LED_Set(uint session, int led, [MarshalAs(UnmanagedType.Bool)] bool on);


    //Switch Functions
    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCodes interfaceIT_Switch_Enable_Callback(uint session, [MarshalAs(UnmanagedType.Bool)] bool enable, KeyNotifyCallback? keyNotifyCallback);

    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCodes interfaceIT_Switch_Enable_Poll(uint session, [MarshalAs(UnmanagedType.Bool)] bool enable);

    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCodes interfaceIT_Switch_Get_Item(uint session, out int key, out int direction);

    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCodes interfaceIT_Switch_Get_State(uint session, out int key, out int state); //Not tested


    //7 Segment Functions
    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCodes interfaceIT_7Segment_Enable(uint session, [MarshalAs(UnmanagedType.Bool)] bool enable);

    [LibraryImport("interfaceITAPI x64.dll", StringMarshalling = StringMarshalling.Utf8)]
    private static partial ErrorCodes interfaceIT_7Segment_Display(uint session, string? data, int start);


    //Dataline Functions
    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCodes interfaceIT_Dataline_Enable(uint session, [MarshalAs(UnmanagedType.Bool)] bool enable);

    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCodes interfaceIT_Dataline_Set(uint session, int dataline, [MarshalAs(UnmanagedType.Bool)] bool on);


    //Brightness Functions
    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCodes interfaceIT_Brightness_Enable(uint session, [MarshalAs(UnmanagedType.Bool)] bool enable);

    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCodes interfaceIT_Brightness_Set(uint session, int brightness);


    //Analog Input Functions
    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCodes interfaceIT_Analog_Enable(uint session, [MarshalAs(UnmanagedType.Bool)] bool enable);

    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCodes interfaceIT_Analog_GetValue(uint session, int reserved, out int pos);

    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCodes interfaceIT_Analog_GetValues(uint session, byte[] values, ref int valuesSize); //Not tested

    //Misc Functions
    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCodes interfaceIT_GetAPIVersion(byte[]? buffer, ref uint bufferSize);


    private static string interfaceIT_GetAPIVersion()
    {
        uint bufferSize = 0;
        interfaceIT_GetAPIVersion(null, ref bufferSize);
        byte[] apiVersion = new byte[bufferSize];
        interfaceIT_GetAPIVersion(apiVersion, ref bufferSize);
        return Encoding.UTF8.GetString(apiVersion);
    }

    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCodes interfaceIT_EnableLogging([MarshalAs(UnmanagedType.Bool)] bool enable);


    private void EnableDeviceFeatures()
    {
        if (HasFeature(Features.SpecialAnalogInput) || HasFeature(Features.SpecialAnalog16Input))
        {
            interfaceIT_Analog_Enable(_session, true);
        }

        if (HasFeature(Features.SpecialBrightness))
        {
            interfaceIT_Brightness_Enable(_session, true);
        }

        if (HasFeature(Features.OutputDataLine))
        {
            interfaceIT_Dataline_Enable(_session, true);
        }

        if (HasFeature(Features.Output7Segment))
        {
            interfaceIT_7Segment_Enable(_session, true);
        }

        if (HasFeature(Features.OutputLed))
        {
            interfaceIT_LED_Enable(_session, true);
        }
    }

    private void DisableDeviceFeatures()
    {
        if (HasFeature(Features.OutputLed))
        {
            for (int i = Led.First; i <= Led.Last; i++)
            {
                interfaceIT_LED_Set(_session, i, false);
            }

            interfaceIT_LED_Enable(_session, false);
        }

        if (HasFeature(Features.InputSwitches))
        {
            interfaceIT_Switch_Enable_Poll(_session, false);
        }

        if (HasFeature(Features.Output7Segment))
        {
            for (int i = SevenSegment.First; i <= SevenSegment.Last; i++)
            {
                interfaceIT_7Segment_Display(_session, null, i);
            }

            interfaceIT_7Segment_Enable(_session, false);
        }

        if (HasFeature(Features.OutputDataLine))
        {
            for (int i = Dataline.First; i <= Dataline.Last; i++)
            {
                interfaceIT_Dataline_Set(_session, i, false);
            }
            
            interfaceIT_Dataline_Enable(_session, false);
        }

        if (HasFeature(Features.SpecialBrightness))
        {
            interfaceIT_Brightness_Set(_session, 0);
            interfaceIT_Brightness_Enable(_session, false);
        }

        if (HasFeature(Features.SpecialAnalogInput) || HasFeature(Features.SpecialAnalog16Input))
        {
            interfaceIT_Analog_Enable(_session, false);
        }
    }

    private bool HasFeature(Features feature)
    {
        return (_features & feature) != 0;
    }

    private static ushort GetProduct(ushort hexCode)
    {
        return (ushort)((hexCode & 0xFF00) >> 8);
    }

    private static ushort GetModel(ushort hexCode)
    {
        return (ushort)((hexCode & 0xFF) >> 0);
    }

    private static InterfaceItBoardId GetInterfaceItBoardId(string boardType)
    {
        return (InterfaceItBoardId)Convert.ToUInt16(boardType, 16);
    }
}