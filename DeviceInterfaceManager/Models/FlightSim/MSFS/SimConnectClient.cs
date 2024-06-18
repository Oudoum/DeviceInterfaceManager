using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

public class SimConnectClient : IFlightSimConnection
{
    private nint _lpPrevWndFunc;
    private WNDPROC _hWndProc = null!;

    private const int WmUserSimConnect = 0x0402;
    private const int MessageSize = 1024;
    private const string ClientDataNameCommand = "DIM.Command";

    public SimConnect? SimConnect { get; private set; }

    public bool IsConnected { get; private set; }
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
                    SimConnect?.ReceiveMessage();
                }
                catch (COMException)
                {
                }
            }
                break;
        }

        return CallWindowProc(_lpPrevWndFunc, hWnd, message, wParam, lParam);
    }

    public async Task ConnectAsync(CancellationToken token)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            IPlatformHandle? platformHandle = desktop.MainWindow?.TryGetPlatformHandle();
            if (platformHandle is not null)
            {
                nint handle = platformHandle.Handle;

                if (_lpPrevWndFunc == default)
                {
                    HWND hWnd = new(handle);
                    _lpPrevWndFunc = PInvoke.GetWindowLongPtr(hWnd, WINDOW_LONG_PTR_INDEX.GWLP_WNDPROC);
                    _hWndProc = HandleSimConnectEvents;
                    PInvoke.SetWindowLongPtr(hWnd, WINDOW_LONG_PTR_INDEX.GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(_hWndProc));
                }

                await CreateSimConnect(handle, token);
            }
        }
    }

    private async Task CreateSimConnect(nint handle, CancellationToken token)
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
                        SimConnect = new SimConnect("Device-Interface-Manager", handle, WmUserSimConnect, null, 0);
                        if (SimConnect is null)
                        {
                            throw new Exception("SimConnect object could not be created");
                        }

                        SimConnect.OnRecvOpen += SimConnectOnOnRecvOpen;
                        SimConnect.OnRecvQuit += SimConnectOnOnRecvQuit;
                        SimConnect.OnRecvException += SimConnectOnOnRecvException;
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
    }

    private void SimConnectOnOnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
    {
        if (SimConnect is null)
        {
            return;
        }
        
        SimConnect.MapClientDataNameToID(ClientDataNameCommand, ClientDataId.Command);
        SimConnect.AddToClientDataDefinition(DefineId.Command, 0, MessageSize, 0, 0);

        RegisterSimVar("CAMERA STATE", "Enum");

        // _simConnect.OnRecvEnumerateInputEvents += SimConnectOnOnRecvEnumerateInputEvents;
        // RegisterSimEvent("ELECTRICAL_BUS_TO_BUS_CONNECTION_TOGGLE");
        //
        // if (_simEvents.TryGetValue("ELECTRICAL_BUS_TO_BUS_CONNECTION_TOGGLE", out int id))
        // {
        //     _simConnect.TransmitClientEvent_EX1(0, (EventId)id, SimConnectGroupPriority.Standard, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY, 2, 8,0,0,0 );
        // }
        
        SimConnect.OnRecvSimobjectData += SimConnectOnOnRecvSimobjectData;
        SimConnect.OnRecvClientData += SimConnectOnOnRecvClientData;

        IsConnected = true;
    }

    private void SimConnectOnOnRecvClientData(SimConnect sender, SIMCONNECT_RECV_CLIENT_DATA data)
    {
        if ((uint)Helper.DataRequestId.Data != data.dwRequestID)
        {
            return;
        }
        
        PmdgHelper.ReceivePmdgData(data.dwData[0]);
    }

    //Test
    private void SimConnectOnOnRecvEnumerateInputEvents(SimConnect sender, SIMCONNECT_RECV_ENUMERATE_INPUT_EVENTS data)
    {
        for (int i = 0; i < data.dwArraySize; i++)
        {
            SIMCONNECT_INPUT_EVENT_DESCRIPTOR descriptor = (SIMCONNECT_INPUT_EVENT_DESCRIPTOR)data.rgData[i];
            _inputEvents.Add(descriptor.Name, descriptor.Hash);
        }

        SimConnect?.SetInputEvent(_inputEvents["FMC_AS01B_1_Keyboard_N"], 1);
    }

    //Test
    private void Test()
    {
        SimConnect?.EnumerateInputEvents((RequestId)100);
    }

    private void SimConnectOnOnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
    {
        Disconnect();
    }

    private readonly Dictionary<string, ulong> _inputEvents = [];

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
        if (SimConnect is null)
        {
            return;
        }

        SimConnect.OnRecvClientData -= SimConnectOnOnRecvClientData;
        SimConnect.OnRecvSimobjectData -= SimConnectOnOnRecvSimobjectData;
        SimConnect.OnRecvException -= SimConnectOnOnRecvException;
        SimConnect.OnRecvQuit -= SimConnectOnOnRecvQuit;
        SimConnect.OnRecvOpen -= SimConnectOnOnRecvOpen;
        SimConnect.Dispose();
        SimConnect = null;

        _simVars.Clear();
        _simEvents.Clear();

        PmdgHelper = new Helper();

        IsConnected = false;
    }

    private void SimConnectOnOnRecvSimobjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
    {
        OnRecvSimobjectData(data.dwRequestID - Offset, (double)data.dwData[0]);
    }

    private void OnRecvSimobjectData(uint requestId, double dwData)
    {
        if (requestId + Offset == 0 || _simVars.Count < requestId)
        {
            return;
        }

        SimVar simVar = _simVars[(int)requestId - 1];
        simVar.Data = dwData;
        OnSimVarChanged?.Invoke(this, simVar);
    }

    // CHANGE TO INTERFACE?!?!
    public event EventHandler<SimVar>? OnSimVarChanged;

    private const int Offset = 4;

    private readonly List<SimVar> _simVars = [];

    private readonly Dictionary<string, int> _simEvents = [];

    private readonly object _lockObject = new();

    // CHANGE TO INTERFACE?!?!
    public void TransmitEvent(uint data, Enum eventId)
    {
        lock (_lockObject)
        {
            SimConnect?.TransmitClientEvent(
                0,
                eventId,
                data,
                SimConnectGroupPriority.Standard,
                SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
        }
    }

    private void TransmitEvent(uint data0, uint data1, Enum eventId)
    {
        lock (_lockObject)
        {
            SimConnect?.TransmitClientEvent_EX1(
                0,
                (EventId)eventId,
                SimConnectGroupPriority.Standard,
                SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY,
                data0,
                data1,
                0,
                0,
                0);
        }
    }

    public void SendWasmEvent(string eventId)
    {
        lock (_lockObject)
        {
            SimConnect?.SetClientData(
                ClientDataId.Command,
                DefineId.Command,
                SIMCONNECT_CLIENT_DATA_SET_FLAG.DEFAULT,
                0,
                new CommandStruct(eventId));
        }
    }

    public void SetSimVar(uint simVarValue, string simVarName)
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
            SimConnect?.SetDataOnSimObject(
                (DefineId)simVar.Id,
                SimConnect.SIMCONNECT_OBJECT_ID_USER,
                SIMCONNECT_DATA_SET_FLAG.DEFAULT,
                simVarValue);
        }
    }

    public double? GetSimVar(string simVarName)
    {
        SimVar? simVar = _simVars.FirstOrDefault(x => x.Name == simVarName);
        return simVar?.Data;
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

        SimConnect?.AddToDataDefinition(
            (DefineId)simVar.Id,
            simVar.Name,
            simVar.Unit,
            SIMCONNECT_DATATYPE.FLOAT64,
            0,
            0);

        SimConnect?.RegisterDataDefineStruct<double>((DefineId)simVar.Id);

        if (request)
        {
            SimConnect?.RequestDataOnSimObject(
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

        SimConnect?.MapClientEventToSimEvent((EventId)id, simEventName);
    }

    public void TransmitSimEvent(uint data0, uint data1, string simEventName)
    {
        if (_simEvents.TryGetValue(simEventName, out int id))
        {
            TransmitEvent(data0, data1, (EventId)id);
        }
    }
    //

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

    private enum SimConnectGroupPriority : uint
    {
        Highest = 1u,
        HighestMaskable = 10000000u,
        Standard = 1900000000u,
        Default = 2000000000u,
        Lowest = 4000000000u
    }

    private enum CameraState
    {
        Cockpit = 2,
        ExternalChase = 3,
        Drone = 4,
        FixedOnPlane = 5,
        Environment = 6,
        SixDoF = 7,
        Gameplay = 8,
        Showcase = 9,
        DroneAircraft = 10,
        Waiting = 11,
        WorldMap = 12,
        HangarRtc = 13,
        HangarCustom = 14,
        MenuRtc = 15,
        InGameRtc = 16,
        Replay = 17,
        DroneTopDown = 19,
        Hangar = 21,
        Ground = 24,
        FollowTrafficAircraft = 25
    }
}