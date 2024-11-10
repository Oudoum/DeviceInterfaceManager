using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.Services.Devices;

public partial class InterfaceItUsbService : DeviceServiceBase
{
    public InterfaceItUsbService()
    {
        Icon = (Geometry?)Application.Current!.FindResource("UsbPort");
    }
    
    public override Task SetLedAsync(int position, bool isEnabled)
    {
        CheckError(interfaceIT_LED_Set(_session, position, isEnabled));
        return Task.CompletedTask;
    }

    public override Task SetDatalineAsync(int position, bool isEnabled)
    {
        CheckError(interfaceIT_Dataline_Set(_session, position, isEnabled));
        return Task.CompletedTask;
    }

    public override Task SetSevenSegmentAsync(int position, string data)
    {
        CheckError(interfaceIT_7Segment_Display(_session, data, position));
        return Task.CompletedTask;
    }

    public override Task SetAnalogAsync(int position, double value)
    {
        if (_features.HasFlag(Features.SpecialBrightness))
        {
            CheckError(interfaceIT_Brightness_Set(_session, (int)Math.Abs(value)));
        }

        return Task.CompletedTask;
    }

    public override Task<ConnectionStatus> ConnectAsync(CancellationToken cancellationToken)
    {
        if (_totalControllers == -1)
        {
            return Task.FromResult(ConnectionStatus.NotConnected);
        }

        foreach (string device in interfaceIT_GetDeviceList())
        {
            if (!_isOpen)
            {
                _isOpen = true;
            }

            if (ErrorCode.ControllerAlreadyBound == interfaceIT_Bind(device, ref _session))
            {
                continue;
            }

            interfaceIT_GetBoardInfo(_session, out BoardInfo boardInfo);
            InterfaceItUsbBoardId usbBoardId = GetInterfaceItBoardId(boardInfo.BoardType);
            Id = device;
            DeviceName = usbBoardId.ToString().Replace('_', ' ');
            _features = boardInfo.Features;

            Inputs.Builder inputsBuilder = new();
            inputsBuilder.SetSwitchInfo(boardInfo.SwitchFirst, boardInfo.SwitchLast);
            if (_features.HasFlag(Features.SpecialAnalogInput))
            {
                inputsBuilder.SetAnalogInfo(1, 1);
            }

            Outputs.Builder outputBuilder = new();
            outputBuilder.SetLedInfo(boardInfo.LedFirst, boardInfo.LedLast).SetDatalineInfo(boardInfo.DatalineFirst, boardInfo.DatalineLast).SetSevenSegmentInfo(boardInfo.SevenSegmentFirst, boardInfo.SevenSegmentLast);
            if (_features.HasFlag(Features.SpecialBrightness))
            {
                outputBuilder.SetAnalogInfo(1, 1);
            }

            Inputs = inputsBuilder.Build();
            Outputs = outputBuilder.Build();

            if (Inputs.Switch.Count <= 64 || usbBoardId == InterfaceItUsbBoardId.FDS_CONTROLLER_MCP)
            {
                CheckError(interfaceIT_SetBoardOptions(_session, (uint)BoardOptions.Force64));
            }

            EnableDeviceFeatures();
            _keyNotifyCallback = KeyPressedProc;
            CheckError(interfaceIT_Switch_Enable_Callback(_session, true, _keyNotifyCallback));

            return Task.FromResult(ConnectionStatus.Connected);
        }

        return Task.FromResult(ConnectionStatus.NotConnected);
    }

    private static bool _isOpen;
    private static int _disconnects;

    public override void Disconnect()
    {
        CheckError(interfaceIT_Switch_Enable_Callback(_session, false, _keyNotifyCallback = null));
        DisableDeviceFeatures();
        CheckError(interfaceIT_UnBind(_session));
        _disconnects++;

        if (_disconnects != TotalControllers && _totalControllers != -1)
        {
            return;
        }

        CheckError(interfaceIT_CloseControllers());
        _isOpen = false;
        _disconnects = 0;
        _totalControllers = -1;
    }

    private KeyNotifyCallback? _keyNotifyCallback;

    private void KeyPressedProc(uint session, int key, uint direction)
    {
        if (Inputs is null)
        {
            return;
        }

        bool isPressed = direction == (int)SwitchDirection.Down;
        OnSwitchPositionChanged(key, isPressed);
    }

    //
    public static int TotalControllers => interfaceIT_GetTotalControllers();

    private Features _features;
    private uint _session;

    private static void CheckError(ErrorCode errorCode)
    {
        if (errorCode != ErrorCode.Ok)
        {
            //Log
        }
    }

    // API
    private delegate void KeyNotifyCallback(uint session, int key, uint direction);

    //Main Functions
    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCode interfaceIT_OpenControllers();

    [LibraryImport("interfaceITAPI x64.dll", StringMarshalling = StringMarshalling.Utf8)]
    private static partial ErrorCode interfaceIT_GetDeviceList(byte[]? buffer, ref uint bufferSize, string? boardType);

    private static IEnumerable<string> interfaceIT_GetDeviceList()
    {
        uint bufferSize = 0;
        CheckError(interfaceIT_GetDeviceList(null, ref bufferSize, null));
        byte[] deviceList = new byte[bufferSize];
        CheckError(interfaceIT_GetDeviceList(deviceList, ref bufferSize, null));
        return Encoding.UTF8.GetString(deviceList).TrimEnd('\0').Split('\0').AsEnumerable();
    }

    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCode interfaceIT_CloseControllers();


    //Controller Functions
    [LibraryImport("interfaceITAPI x64.dll", StringMarshalling = StringMarshalling.Utf8)]
    private static partial ErrorCode interfaceIT_Bind(string controller, ref uint session);

    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCode interfaceIT_UnBind(uint session);

    [DllImport("interfaceITAPI x64.dll")]
    [SuppressMessage("Interoperability", "SYSLIB1054:Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time", Justification = "<Pending>")]
    private static extern ErrorCode interfaceIT_GetBoardInfo(uint session, out BoardInfo boardInfo);

    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCode interfaceIT_SetBoardOptions(uint session, uint options);

    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCode interfaceIT_GetTotalControllers(ref int controllerCount);


    private static int _totalControllers = -1;

    private static int interfaceIT_GetTotalControllers()
    {
        CheckError(interfaceIT_OpenControllers());
        CheckError(interfaceIT_GetTotalControllers(ref _totalControllers));
        return _totalControllers;
    }


    //LED Functions
    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCode interfaceIT_LED_Enable(uint session, [MarshalAs(UnmanagedType.Bool)] bool enable);

    // [LibraryImport("interfaceITAPI x64.dll")]
    // private static partial ErrorCode interfaceIT_LED_Test(uint session, [MarshalAs(UnmanagedType.Bool)] bool enable);

    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCode interfaceIT_LED_Set(uint session, int led, [MarshalAs(UnmanagedType.Bool)] bool on);


    //Switch Functions
    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCode interfaceIT_Switch_Enable_Callback(uint session, [MarshalAs(UnmanagedType.Bool)] bool enable, KeyNotifyCallback? keyNotifyCallback);

    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCode interfaceIT_Switch_Enable_Poll(uint session, [MarshalAs(UnmanagedType.Bool)] bool enable);

    // [LibraryImport("interfaceITAPI x64.dll")]
    // private static partial ErrorCode interfaceIT_Switch_Get_Item(uint session, out int key, out int direction);

    // [LibraryImport("interfaceITAPI x64.dll")]
    // private static partial ErrorCode interfaceIT_Switch_Get_State(uint session, out int key, out int state); //Not tested


    //7 Segment Functions
    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCode interfaceIT_7Segment_Enable(uint session, [MarshalAs(UnmanagedType.Bool)] bool enable);

    [LibraryImport("interfaceITAPI x64.dll", StringMarshalling = StringMarshalling.Utf8)]
    private static partial ErrorCode interfaceIT_7Segment_Display(uint session, string? data, int start);


    //Dataline Functions
    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCode interfaceIT_Dataline_Enable(uint session, [MarshalAs(UnmanagedType.Bool)] bool enable);

    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCode interfaceIT_Dataline_Set(uint session, int dataline, [MarshalAs(UnmanagedType.Bool)] bool on);


    //Brightness Functions
    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCode interfaceIT_Brightness_Enable(uint session, [MarshalAs(UnmanagedType.Bool)] bool enable);

    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCode interfaceIT_Brightness_Set(uint session, int brightness);


    //Analog Input Functions
    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCode interfaceIT_Analog_Enable(uint session, [MarshalAs(UnmanagedType.Bool)] bool enable);

    [LibraryImport("interfaceITAPI x64.dll")]
    private static partial ErrorCode interfaceIT_Analog_GetValue(uint session, int reserved, out int value);

    // [LibraryImport("interfaceITAPI x64.dll")]
    // private static partial ErrorCode interfaceIT_Analog_GetValues(uint session, byte[] values, ref int valuesSize); //Not tested

    // //Misc Functions
    // [LibraryImport("interfaceITAPI x64.dll")]
    // private static partial ErrorCode interfaceIT_GetAPIVersion(byte[]? buffer, ref uint bufferSize);


    // private static string interfaceIT_GetAPIVersion()
    // {
    //     uint bufferSize = 0;
    //     interfaceIT_GetAPIVersion(null, ref bufferSize);
    //     byte[] apiVersion = new byte[bufferSize];
    //     interfaceIT_GetAPIVersion(apiVersion, ref bufferSize);
    //     return Encoding.UTF8.GetString(apiVersion);
    // }

    // [LibraryImport("interfaceITAPI x64.dll")]
    // private static partial ErrorCode interfaceIT_EnableLogging([MarshalAs(UnmanagedType.Bool)] bool enable);

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

    private enum ErrorCode : short
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
        InputReserved2 = 0x00000002,
        InputReserved3 = 0x00000004,
        InputReserved4 = 0x00000008,
        InputReserved5 = 0x00000010,
        InputReserved6 = 0x00000020,
        InputReserved7 = 0x00000040,
        InputReserved8 = 0x00000080,

        OutputLed = 0x00000100,
        OutputReserved11 = 0x00000200,
        Output7Segment = 0x00000400,
        OutputReserved12 = 0x00000800,
        OutputReserved13 = 0x00001000,
        OutputDataLine = 0x00002000,
        OutputReserved15 = 0x00004000,
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

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public readonly int[] Reserved;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
        public readonly string BoardType;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public readonly string ManufactureDate;

        public readonly Features Features;
        public readonly int UpdateLevel;
    }
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    private void EnableDeviceFeatures()
    {
        if (HasFeature(Features.SpecialAnalogInput) || HasFeature(Features.SpecialAnalog16Input))
        {
            CheckError(interfaceIT_Analog_Enable(_session, true));
        }

        if (HasFeature(Features.SpecialBrightness))
        {
            CheckError(interfaceIT_Brightness_Enable(_session, true));
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => GetAnalogValueAsync(_cancellationTokenSource.Token));
        }

        if (HasFeature(Features.OutputDataLine))
        {
            CheckError(interfaceIT_Dataline_Enable(_session, true));
        }

        if (HasFeature(Features.Output7Segment))
        {
            CheckError(interfaceIT_7Segment_Enable(_session, true));
        }

        if (HasFeature(Features.OutputLed))
        {
            CheckError(interfaceIT_LED_Enable(_session, true));
        }
    }

    private CancellationTokenSource? _cancellationTokenSource;

    private async Task GetAnalogValueAsync(CancellationToken cancellationToken)
    {
        if (Inputs is null)
        {
            return;
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            CheckError(interfaceIT_Analog_GetValue(_session, 0, out int value));
            OnAnalogInValueChanged(Inputs.Analog.First, value);
            await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
        }
    }

    private void DisableDeviceFeatures()
    {
        if (Outputs is null)
        {
            return;
        }

        if (HasFeature(Features.OutputLed))
        {
            for (int i = Outputs.Led.First; i <= Outputs.Led.Last; i++)
            {
                CheckError(interfaceIT_LED_Set(_session, i, false));
            }

            CheckError(interfaceIT_LED_Enable(_session, false));
        }

        if (HasFeature(Features.InputSwitches))
        {
            CheckError(interfaceIT_Switch_Enable_Poll(_session, false));
        }

        if (HasFeature(Features.Output7Segment))
        {
            for (int i = Outputs.SevenSegment.First; i <= Outputs.SevenSegment.Last; i++)
            {
                CheckError(interfaceIT_7Segment_Display(_session, null, i));
            }

            CheckError(interfaceIT_7Segment_Enable(_session, false));
        }

        if (HasFeature(Features.OutputDataLine))
        {
            for (int i = Outputs.Dataline.First; i <= Outputs.Dataline.Last; i++)
            {
                CheckError(interfaceIT_Dataline_Set(_session, i, false));
            }

            CheckError(interfaceIT_Dataline_Enable(_session, false));
        }

        if (HasFeature(Features.SpecialBrightness))
        {
            CheckError(interfaceIT_Brightness_Set(_session, 0));
            CheckError(interfaceIT_Brightness_Enable(_session, false));
        }

        if (HasFeature(Features.SpecialAnalogInput) || HasFeature(Features.SpecialAnalog16Input))
        {
            _cancellationTokenSource?.Cancel();
            CheckError(interfaceIT_Analog_Enable(_session, false));
        }
    }

    private bool HasFeature(Features feature)
    {
        return (_features & feature) != 0;
    }

    private static InterfaceItUsbBoardId GetInterfaceItBoardId(string boardType)
    {
        if (string.IsNullOrEmpty(boardType))
        {
            return 0;
        }

        return (InterfaceItUsbBoardId)Convert.ToUInt16(boardType, 16);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    private enum InterfaceItUsbBoardId : ushort
    {
        //Board type identifiers
        INTERFACEIT_BOARD_ALL = 0,

        // Misc
        // FDS-MFP - Original version
        FDS_MFP = 0x0201,

        // FDS-SYS boards - Older original boards
        FDSSYS1 = 0x32E1,

        FDSSYS2 = 0x32E2,

        FDSSYS3 = 0x32E0,

        FDSSYS4 = 0x32E3,

        //Radios
        // Base
        RADIO_CODE = 0x4C00,

        // 737 NAV v1
        FDS_737NG_NAV = 0x4C55,
        FDS_737NG_NAV_ID = 0x0055,

        // 737 COMM v1
        FDS_737NG_COMM = 0x4C56,
        FDS_737NG_COMM_ID = 0x0056,

        // A320 Multi v1
        FDS_A320_MULTI = 0x4C57,
        FDS_A320_MULTI_ID = 0x0057,

        // 737 NAV v2
        FDS_737NG_NAV_V2 = 0x4C58,
        FDS_737NG_NAV_V2_ID = 0x0058,

        // 737 COMM v2
        FDS_737NG_COMM_V2 = 0x4C59,
        FDS_737NG_COMM_V2_ID = 0x0059,

        // 737 NAV / COMM
        FDS_737NG_NAVCOMM = 0x4C5A,
        FDS_737NG_NAVCOMM_ID = 0x005A,

        // FDS-CDU
        FDS_CDU = 0x3302,

        // FDS-A-ACP
        FDS_A_ACP = 0x3303,

        // FDS-A-RMP
        FDS_A_RMP = 0x3304,

        // FDS-XPNDR
        FDS_XPNDR = 0x3305,

        // FDS-737NG-ELECT
        FDS_737_ELECT = 0x3306,

        // FS-MFP v2
        MFP_V2 = 0x3307,

        // FDS-CONTROLLER-MCP
        FDS_CONTROLLER_MCP = 0x330A,

        // FDS-CONTROLLER-EFIS-CA
        FDS_CONTROLLER_EFIS_CA = 0x330B,

        // FDS-CONTROLLER-EFIS-FO
        FDS_CONTROLLER_EFIS_FO = 0x330C,

        // FDS-737NG-MCP-ELVL - Not production
        FDS_737NG_MCP_ELVL = 0x3310,

        // FDS-737-EL-MCP / FDS-737-MX-MCP
        FDS_737_MX_MCP = 0x3311,

        // FDS-787-MCP
        FDS_787NG_MCP = 0x3319,

        // FDS-A320-FCU
        FDS_A320_FCU = 0x3316,

        // FDS_747_RADIO (MULTI_COMM)
        FDS_7X7_MCOMM = 0x3318,

        // FDS-CDU v9

        FDS_CDU_V9 = 0x331A,

        // FDS-A-TCAS V1

        FDS_A_TCAS = 0x331B,

        // FDS-A-ECAM v1

        FDS_A_ECAM = 0x331C,

        // FDS-A-CLOCK v1
        FDS_A_CLOCK = 0x331D,

        FDS_A_RMP_V2 = 0x331E,

        FDS_A_ACP_V2 = 0x331F,

        A320_PEDESTAL = 0x3320,

        FDS_A320_35VU = 0x3321,

        FDS_IRS = 0x3322,

        FDS_OM1 = 0x3323,

        FDS_OE1 = 0x3324,

        FDS_GM1 = 0x3325,

        FDS_NDF_GDS_NCP = 0x3326,

        FDS_777_MX_MCP = 0x3327,

        FDS_DCP_EFIS = 0x3328,

        FDS_747_MX_MCP = 0x3329,

        // 5 Position EFIS
        FDS_737_PMX_EFIS_5_CA = 0x332A,

        // 5 Position EFIS
        FDS_737_PMX_EFIS_5_FO = 0x332B,

        // 737 Pro MX MCP
        FDS_737_PMX_MCP = 0x332C,

        // 737 Pro MX EFIS (Encoder) - CA
        FDS_737_PMX_EFIS_E_CA = 0x332D,

        // 737 Pro MX EFIS (Encoder) - FO
        FDS_737_PMX_EFIS_E_FO = 0x332E,

        // 737 MAX 
        FDS_737_MAX_ABRAKE_EFIS = 0x332F,

        // 787 Tuning and Control Panel
        FDS_787_TCP = 0x3330,

        // C17 AFCSP
        FDS_C17_AFCSP = 0x33EF,

        // JetMAX Boards
        JetMAX_737_MCP = 0x330F,

        JetMAX_737_RADIO = 0x3401,

        JetMAX_737_XPNDR = 0x3402,

        JetMAX_777_MCP = 0x3403,

        JetMAX_737_MCP_V2 = 0x3404,

        // interfaceIT™ Boards
        IIT_HIO_32_64 = 0x4101,

        IIT_HIO_64_128 = 0x4102,

        IIT_HIO_128_256 = 0x4103,

        IIT_HI_128 = 0x4105,

        IIT_HRI_8 = 0x4106,

        IIT_RELAY_8 = 0x4107,

        IIT_DEV = 0x4108,

        HIO_RELAY_8 = 0x4109
    }
}