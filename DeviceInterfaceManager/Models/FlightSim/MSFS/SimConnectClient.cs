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

    private SimConnect? _simConnect;

    public bool IsConnected { get; private set; }

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
                        _simConnect = new SimConnect("Device-Interface-Manager", handle, WmUserSimConnect, null, 0);
                        if (_simConnect is null)
                        {
                            throw new Exception("SimConnect object could not be created");
                        }

                        _simConnect.OnRecvOpen += SimConnectOnOnRecvOpen;
                        _simConnect.OnRecvQuit += SimConnectOnOnRecvQuit;
                        _simConnect.OnRecvException += SimConnectOnOnRecvException;
                        Helper = new Helper(_simConnect);
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


    public Helper? Helper { get; private set; }

    private void SimConnectOnOnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
    {
        if (_simConnect is null)
        {
            return;
        }

        // Test();

        _simConnect.MapClientDataNameToID(ClientDataNameCommand, ClientDataId.Command);
        _simConnect.AddToClientDataDefinition(DefineId.Command, 0, MessageSize, 0, 0);

        Helper?.RegisterB737DataAndEvents();

        RegisterSimVar("CAMERA STATE", "Enum");

        // _simConnect.OnRecvEnumerateInputEvents += SimConnectOnOnRecvEnumerateInputEvents;

        _simConnect.OnRecvSimobjectData += SimConnectOnOnRecvSimobjectData;

        IsConnected = true;
    }

    private void SimConnectOnOnRecvEnumerateInputEvents(SimConnect sender, SIMCONNECT_RECV_ENUMERATE_INPUT_EVENTS data)
    {
        for (int i = 0; i < data.dwArraySize; i++)
        {
            SIMCONNECT_INPUT_EVENT_DESCRIPTOR descriptor = (SIMCONNECT_INPUT_EVENT_DESCRIPTOR)data.rgData[i];
            _inputEvents.Add(descriptor.Name, descriptor.Hash);
        }

        _simConnect?.SetInputEvent(_inputEvents["FMC_AS01B_1_Keyboard_N"], 1);
    }

    private void Test()
    {
        _simConnect?.EnumerateInputEvents((RequestId)100);
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
        if (_simConnect is null)
        {
            return;
        }

        _simConnect.OnRecvException -= SimConnectOnOnRecvException;
        _simConnect.OnRecvQuit -= SimConnectOnOnRecvQuit;
        _simConnect.OnRecvOpen -= SimConnectOnOnRecvOpen;
        _simConnect.Dispose();
        _simConnect = null;

        _simVars.Clear();
        _simEvents.Clear();

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
            _simConnect?.TransmitClientEvent(
                0,
                eventId,
                data,
                SimConnectGroupPriority.Standard,
                SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
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
            _simConnect?.SetDataOnSimObject(
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

    public void TransmitSimEvent(uint data, string simEventName)
    {
        if (_simEvents.TryGetValue(simEventName, out int id))
        {
            TransmitEvent(data, (EventId)id);
        }
    }
    //

    public class SimVar
    {
        public uint Id { get; }

        public string Name { get; }

        public double Data { get; set; }

        public string? Unit { get; }

        public SimVar(uint id, string name, string? unit)
        {
            Id = id;
            Name = name;
            Unit = unit;
        }

        public SimVar(string name, double data)
        {
            Name = name;
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