using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using DeviceInterfaceManager.Models.FlightSim.MSFS.PMDG.SDK;
using Microsoft.FlightSimulator.SimConnect;

namespace DeviceInterfaceManager.Models.FlightSim.MSFS;

public class SimConnectClient
{
    private nint _lpPrevWndFunc;
    private WNDPROC _hWndProc = null!;

    private const int WmUserSimConnect = 0x0402;
    private const int MessageSize = 1024;
    private const string ClientDataNameCommand = "DIM.Command";

    private SimConnect? _simConnect;
    private string? _aircraftTitle;
    private readonly SignalRClientService _signalRClientService;
    public Helper PmdgHelper { get; private set; } = new();

    [DllImport("User32.dll", SetLastError = true)]
#pragma warning disable SYSLIB1054
    private static extern LRESULT CallWindowProc(nint lpPrevWndFunc, HWND hWnd, uint message, WPARAM wParam, LPARAM lParam);
#pragma warning restore SYSLIB1054

    private LRESULT HandleSimConnectEvents(HWND hWnd, uint message, WPARAM wParam, LPARAM lParam)
    {
        switch (message)
        {
            case WmUserSimConnect:
            {
                try
                {
                    _simConnect?.ReceiveMessage();
                }
                catch (COMException)
                {
                }
            }
                break;
        }

        return CallWindowProc(_lpPrevWndFunc, hWnd, message, wParam, lParam);
    }

    public SimConnectClient(SignalRClientService signalRClientService)
    {
        _signalRClientService = signalRClientService;
        _signalRClientService.Connected += () =>
        {
            RequestTitle();
            ResendCduData();
        };
    }

    public async Task<string?> ConnectAsync(CancellationToken token)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return null;
        }

        IPlatformHandle? platformHandle = desktop.MainWindow?.TryGetPlatformHandle();
        if (platformHandle is null)
        {
            return null;
        }

        nint handle = platformHandle.Handle;

        if (_lpPrevWndFunc != default)
        {
            return await CreateSimConnect(handle, token);
        }

        HWND hWnd = new(handle);
        _lpPrevWndFunc = PInvoke.GetWindowLongPtr(hWnd, WINDOW_LONG_PTR_INDEX.GWLP_WNDPROC);
        _hWndProc = HandleSimConnectEvents;
        PInvoke.SetWindowLongPtr(hWnd, WINDOW_LONG_PTR_INDEX.GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(_hWndProc));
        
        return await CreateSimConnect(handle, token);
    }

    private async Task<string?> CreateSimConnect(nint handle, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            TaskCompletionSource<bool> tcs = new();
            try
            {
                await Task.Run(() =>
                {
                    try
                    {
                        _simConnect = new SimConnect("Device-Interface-Manager", handle, WmUserSimConnect, null, 0);
                        if (_simConnect is null)
                        {
                            throw new Exception("SimConnect object could not be created");
                        }

                        _simConnect.OnRecvOpen += SimConnectOnOnRecvOpen;
                        _simConnect.OnRecvQuit += SimConnectOnOnRecvQuit;
                        _simConnect.OnRecvException += SimConnectOnOnRecvException;
                        tcs.SetResult(true);
                    }
                    catch (Exception)
                    {
                        tcs.SetResult(false);
                    }
                }, token);

                if (!await tcs.Task)
                {
                    await Task.Delay(1000, token);
                }
                else
                {
                    break;
                }
            }
            catch (TaskCanceledException)
            {
            }
        }

        await Task.Run(() =>
        {
            while (_aircraftTitle is null)
            {
            }
        }, token);
        await _signalRClientService.SendTitleMessageAsync(_aircraftTitle);
        return _aircraftTitle;
    }

    private void SimConnectOnOnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
    {
        if (_simConnect is null)
        {
            return;
        }

        _simConnect.MapClientDataNameToID(ClientDataNameCommand, ClientDataId.Command);
        _simConnect.AddToClientDataDefinition(DefineId.Command, 0, MessageSize, 0, 0);
        _simConnect.AddToDataDefinition(
            (DefineId)6,
            "TITLE",
            null,
            SIMCONNECT_DATATYPE.STRING256,
            0,
            0);
        _simConnect.RegisterDataDefineStruct<String256>((DefineId)6);
        RequestTitle();

        RegisterSimVar("CAMERA STATE", "Enum");

        // _simConnect.OnRecvEnumerateInputEvents += SimConnectOnOnRecvEnumerateInputEvents;
        // RegisterSimEvent("ELECTRICAL_BUS_TO_BUS_CONNECTION_TOGGLE");
        //
        // if (_simEvents.TryGetValue("ELECTRICAL_BUS_TO_BUS_CONNECTION_TOGGLE", out int id))
        // {
        //     _simConnect.TransmitClientEvent_EX1(0, (EventId)id, SimConnectGroupPriority.Standard, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY, 2, 8,0,0,0 );
        // }

        _simConnect.OnRecvSimobjectData += SimConnectOnOnRecvSimobjectData;
        _simConnect.OnRecvClientData += SimConnectOnOnRecvClientData;
    }

    private void RequestTitle()
    {
        _simConnect?.RequestDataOnSimObject((RequestId)6, (DefineId)6, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.ONCE, 0, 0, 0, 0);
    }
    
    private void ResendCduData()
    {
        if (_simConnect is null)
        {
            return;
        }
        
        PmdgHelper.RequestClientDataOnce(_simConnect);
    }

    private void SimConnectOnOnRecvClientData(SimConnect sender, SIMCONNECT_RECV_CLIENT_DATA data)
    {
        Helper.DataRequestId dataRequestId = (Helper.DataRequestId)data.dwRequestID;
        switch (dataRequestId)
        {
            case Helper.DataRequestId.Data:
                PmdgHelper.ReceivePmdgData(data.dwData[0]);
                break;

            case Helper.DataRequestId.Cdu0:
            case Helper.DataRequestId.Cdu1:
            case Helper.DataRequestId.Cdu2:
                _ = _signalRClientService.SendPmdgDataMessageAsync((byte)dataRequestId, ((Cdu.ScreenBytes)data.dwData[0]).Data);
                break;
        }
    }

    //Test
    // private void SimConnectOnOnRecvEnumerateInputEvents(SimConnect sender, SIMCONNECT_RECV_ENUMERATE_INPUT_EVENTS data)
    // {
    //     for (int i = 0; i < data.dwArraySize; i++)
    //     {
    //         SIMCONNECT_INPUT_EVENT_DESCRIPTOR descriptor = (SIMCONNECT_INPUT_EVENT_DESCRIPTOR)data.rgData[i];
    //         _inputEvents.Add(descriptor.Name, descriptor.Hash);
    //     }
    //
    //     _simConnect?.SetInputEvent(_inputEvents["FMC_AS01B_1_Keyboard_N"], 1);
    // }
    //Test
    // private void Test()
    // {
    //     _simConnect?.EnumerateInputEvents((RequestId)100);
    // }

    private void SimConnectOnOnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
    {
        Disconnect();
    }

    // private readonly Dictionary<string, ulong> _inputEvents = [];

    private static void SimConnectOnOnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
    {
        foreach (object? item in Enum.GetValues(typeof(SIMCONNECT_EXCEPTION)))
        {
            if ((int)(object)item == data.dwException)
            {
                Debug.WriteLine(item);
            }
        }
    }

    public void Disconnect()
    {
        if (_simConnect is null)
        {
            return;
        }

        _simConnect.OnRecvClientData -= SimConnectOnOnRecvClientData;
        _simConnect.OnRecvSimobjectData -= SimConnectOnOnRecvSimobjectData;
        _simConnect.OnRecvException -= SimConnectOnOnRecvException;
        _simConnect.OnRecvQuit -= SimConnectOnOnRecvQuit;
        _simConnect.OnRecvOpen -= SimConnectOnOnRecvOpen;
        _simConnect.Dispose();
        _simConnect = null;

        _simVars.Clear();
        _simEvents.Clear();

        PmdgHelper = new Helper();

        _aircraftTitle = null;
    }
    
    private void SimConnectOnOnRecvSimobjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
    {
        if (_simConnect is not null && data.dwRequestID == 6)
        {
            _ = _signalRClientService.SendTitleMessageAsync(_aircraftTitle);
            
            _aircraftTitle = ((String256)data.dwData[0]).value;
            if (_aircraftTitle.StartsWith("PMDG 737"))
            {
                PmdgHelper.InitializePmdg737(_simConnect);
            }
            else if (_aircraftTitle.StartsWith("PMDG 777"))
            {
                PmdgHelper.InitializePmdg777(_simConnect);
            }

            return;
        }

        OnRecvSimobjectData(data.dwRequestID - Offset, (double)data.dwData[0]);
    }

    private void OnRecvSimobjectData(uint requestId, double dwData)
    {
        if (requestId + Offset == 0 || _simVars.Count < requestId)
        {
            return;
        }

        SimVar simVar = _simVars[(int)requestId - 1];
        simVar.Data = Math.Round(dwData, 9);
        OnSimVarChanged?.Invoke(this, simVar);
    }

    public event EventHandler<SimVar>? OnSimVarChanged;

    private const int Offset = 8;

    private readonly List<SimVar> _simVars = [];

    private readonly Dictionary<string, int> _simEvents = [];

    private readonly object _lockObject = new();

    public void TransmitEvent(long data, Enum eventId)
    {
        lock (_lockObject)
        {
            _simConnect?.TransmitClientEvent(
                0,
                eventId,
                (uint)data,
                SimConnectGroupPriority.Standard,
                SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
        }
    }

    private void TransmitEvent(long data0, long data1, Enum eventId)
    {
        lock (_lockObject)
        {
            _simConnect?.TransmitClientEvent_EX1(
                0,
                (EventId)eventId,
                SimConnectGroupPriority.Standard,
                SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY,
                (uint)data0,
                (uint)data1,
                0,
                0,
                0);
        }
    }

    public void SendWasmEvent(string eventId)
    {
        lock (_lockObject)
        {
            _simConnect?.SetClientData(
                ClientDataId.Command,
                DefineId.Command,
                SIMCONNECT_CLIENT_DATA_SET_FLAG.DEFAULT,
                0,
                new CommandStruct(eventId));
        }
    }

    public void SetSimVar(long simVarValue, string simVarName)
    {
        SetSimVars(simVarName, null, simVarValue);
    }

    private void SetSimVars(string simVarName, string? simVarUnit, double simVarValue)
    {
        if (!_simVars.Exists(x => x.Name == simVarName))
        {
            RegisterSimVars(simVarName, simVarUnit, false);
        }

        SimVar? simVar = _simVars.Find(x => x.Name == simVarName);
        if (simVar is null)
        {
            return;
        }

        lock (_lockObject)
        {
            _simConnect?.SetDataOnSimObject(
                (DefineId)simVar.Id,
                SimConnect.SIMCONNECT_OBJECT_ID_USER,
                SIMCONNECT_DATA_SET_FLAG.DEFAULT,
                simVarValue);
        }
    }

    public void RegisterSimVar(string simVarName)
    {
        RegisterSimVars(simVarName, null, true);
    }

    public void RegisterSimVar(string simVarName, string simVarUnit)
    {
        RegisterSimVars(simVarName, simVarUnit, true);
    }

    private void RegisterSimVars(string simVarName, string? simVarUnit, bool request)
    {
        if (_simVars.Exists(x => x.Name == simVarName))
        {
            return;
        }

        SimVar simVar = new((uint)(_simVars.Count + Offset + 1), simVarName, simVarUnit);
        _simVars.Add(simVar);

        _simConnect?.AddToDataDefinition(
            (DefineId)simVar.Id,
            simVar.Name,
            simVar.Unit,
            SIMCONNECT_DATATYPE.FLOAT64,
            0,
            0);

        _simConnect?.RegisterDataDefineStruct<double>((DefineId)simVar.Id);

        if (request)
        {
            _simConnect?.RequestDataOnSimObject(
                (RequestId)simVar.Id,
                (DefineId)simVar.Id,
                SimConnect.SIMCONNECT_OBJECT_ID_USER,
                SIMCONNECT_PERIOD.SIM_FRAME,
                SIMCONNECT_DATA_REQUEST_FLAG.CHANGED,
                0,
                0,
                0);
        }
    }

    public void RegisterSimEvent(string simEventName)
    {
        if (_simEvents.ContainsKey(simEventName))
        {
            return;
        }

        int id = _simEvents.Count + 1;
        _simEvents.Add(simEventName, id);

        _simConnect?.MapClientEventToSimEvent((EventId)id, simEventName);
    }

    public void TransmitSimEvent(long data0, long data1, string simEventName)
    {
        if (_simEvents.TryGetValue(simEventName, out int id))
        {
            TransmitEvent(data0, data1, (EventId)id);
        }
    }

    public void TransmitSimEvent(string simEventName)
    {
        if (_simEvents.TryGetValue(simEventName, out int id))
        {
            TransmitEvent(0, (EventId)id);
        }
    }

    public class SimVar(uint id, string name, string? unit)
    {
        public uint Id { get; } = id;

        public string Name { get; } = name;

        public double Data { get; set; }

        public string? Unit { get; } = unit;

        public SimVar(string name, double data) : this(0, name, null)
        {
            Data = data;
        }
    }

    private enum ClientDataId
    {
        Command
    }

    private enum RequestId;

    private enum DefineId
    {
        Command
    }

    private enum EventId;

    private struct CommandStruct(string command)
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageSize)]
        private string _command = command;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    private struct String256
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string value;
    };

    private enum SimConnectGroupPriority : uint
    {
        Highest = 1u,
        HighestMaskable = 10000000u,
        Standard = 1900000000u,
        Default = 2000000000u,
        Lowest = 4000000000u
    }
}