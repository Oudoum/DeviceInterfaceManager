using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using DeviceInterfaceManager.Models.FlightSim.MSFS.PMDG;
using Microsoft.FlightSimulator.SimConnect;

namespace DeviceInterfaceManager.Services;

public class SimConnectClientService
{
    private const int WmUserSimConnect = 0x0402;
    private const int MessageSize = 1024;
    private const string ClientDataNameCommand = "DIM.Command";

    private SimConnect? _simConnect;
    private string? _aircraftTitle;
    private readonly SignalRClientService _signalRClientService;
    private readonly PmdgHelperService _pmdgHelperService;
    
    public Action<string?>? AircraftTitleChanged;
    
    public SimConnectClientService(PmdgHelperService pmdgHelperService, SignalRClientService signalRClientService)
    {
        _pmdgHelperService = pmdgHelperService;
        _signalRClientService = signalRClientService;
        _signalRClientService.Connected += () =>
        {
            RequestTitle();
            ResendCduData();
        };
    }

    private IntPtr CustomWndProcHookCallback(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        switch (msg)
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

        return IntPtr.Zero;
    }

    public async Task<string?> ConnectAsync(CancellationToken token)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return null;
        }

        IPlatformHandle? platformHandle = desktop.MainWindow?.TryGetPlatformHandle();
        if (platformHandle is null || desktop.MainWindow is null)
        {
            return null;
        }

        Win32Properties.AddWndProcHookCallback(desktop.MainWindow, CustomWndProcHookCallback);
        return await CreateSimConnect(platformHandle.Handle, token);
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
                        while (_aircraftTitle is null)
                        {
                            
                        }

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
        
        _pmdgHelperService.RequestClientDataOnce(_simConnect);
    }

    private void SimConnectOnOnRecvClientData(SimConnect sender, SIMCONNECT_RECV_CLIENT_DATA data)
    {
        PmdgHelperService.DataRequestId dataRequestId = (PmdgHelperService.DataRequestId)data.dwRequestID;
        switch (dataRequestId)
        {
            case PmdgHelperService.DataRequestId.Data:
                _pmdgHelperService.ReceivePmdgData(data.dwData[0]);
                break;

            case PmdgHelperService.DataRequestId.Cdu0:
            case PmdgHelperService.DataRequestId.Cdu1:
            case PmdgHelperService.DataRequestId.Cdu2:
                _ = _signalRClientService.SendPmdgDataMessageAsync((byte)dataRequestId, ((Cdu.ScreenBytes)data.dwData[0]).Data);
                break;
        }
    }

    private void SimConnectOnOnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
    {
        Disconnect();
    }

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
        _aircraftTitle = null;
        
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop || desktop.MainWindow is null)
        {
            return;
        }
        
        Win32Properties.RemoveWndProcHookCallback(desktop.MainWindow, CustomWndProcHookCallback);
    }
    
    private void SimConnectOnOnRecvSimobjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
    {
        if (_simConnect is not null && data.dwRequestID == 6)
        {
            _aircraftTitle = ((String256)data.dwData[0]).value;
            AircraftTitleChanged?.Invoke(_aircraftTitle);
            _ = _signalRClientService.SendTitleMessageAsync(_aircraftTitle);
            
            if (_aircraftTitle.StartsWith("PMDG 737"))
            {
                _pmdgHelperService.InitializePmdg737(_simConnect);
            }
            else if (_aircraftTitle.StartsWith("PMDG 777"))
            {
                _pmdgHelperService.InitializePmdg777(_simConnect);
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

    public void TransmitEvent(double data, Enum eventId)
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

    private void TransmitEvent(double data0, double data1, Enum eventId)
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

    public void SetSimVar(double simVarValue, string simVarName)
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

    public void TransmitSimEvent(double data0, double data1, string simEventName)
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