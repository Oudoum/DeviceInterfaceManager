using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.FlightSimulator.SimConnect;

namespace DeviceInterfaceManager.Models.FlightSim.MSFS.PMDG.SDK;

public class Helper
{
    private readonly SimConnect _simConnect;
    
    public Helper(SimConnect simConnect)
    {
        _simConnect = simConnect;
        _simConnect.OnRecvClientData += SimConnectOnOnRecvClientData;
    }
    
    public event EventHandler<PmdgDataFieldChangedEventArgs>? FieldChanged;

    private readonly B737.Data _data = new();

    public List<string> WatchedFields { get; } = [];

    public IDictionary<string, object?> DynDict { get; } = new ExpandoObject();

    private bool _initialized;
    
    private void SimConnectOnOnRecvClientData(SimConnect sender, SIMCONNECT_RECV_CLIENT_DATA data)
    {
        if ((uint)DataRequestId.Data == data.dwRequestID)
        {
            Iteration((B737.Data)data.dwData[0]);
        }
    }
    
    public void Init(ProfileCreatorModel profileCreatorModel)
    {
        foreach (OutputCreator output in profileCreatorModel.OutputCreators)
        {
            if (output is not { DataType: ProfileCreatorModel.Pmdg737, IsActive: true })
            {
                continue;
            }

            if (!string.IsNullOrEmpty(output.PmdgData))
            {
                string? pmdgDataFieldName = ConvertDataToPmdgDataFieldName(output);
                if (pmdgDataFieldName is not null && !WatchedFields.Contains(pmdgDataFieldName))
                {
                    WatchedFields.Add(pmdgDataFieldName);
                }
            }

            if (_initialized)
            {
                continue;
            }

            _initialized = true;
            Initialize<B737.Data>();
        }
    }
    
    public static string? ConvertDataToPmdgDataFieldName(OutputCreator output)
    {
        string? pmdgDataFieldName = output.PmdgData;
        if (output.PmdgDataArrayIndex is not null)
        {
            pmdgDataFieldName = pmdgDataFieldName + '_' + output.PmdgDataArrayIndex;
        }
        return pmdgDataFieldName;
    }
    
    private void Initialize<T>() where T : struct
    {
        foreach (FieldInfo field in typeof(T).GetFields())
        {
            if (field.Name == "reserved")
            {
                continue;
            }

            if (field.FieldType.IsArray)
            {
                Array array = (Array)field.GetValue(_data)!;
                if (field.GetCustomAttributes(typeof(MarshalAsAttribute), false).FirstOrDefault() is MarshalAsAttribute marshalAsAttribute)
                {
                    array = Array.CreateInstance(field.FieldType.GetElementType()!, marshalAsAttribute.SizeConst);
                    field.SetValue(_data, array);
                }
                int i = 0;
                foreach (object item in array)
                {
                    DynDict[field.Name + '_' + i] = item;
                    i++;
                }
            }
            
            DynDict[field.Name] = field.GetValue(_data);
        }
    }
    
    private void Iteration<T>(T newData) where T : struct
    {
        foreach (FieldInfo field in typeof(T).GetFields())
        {
            if (field.Name == "reserved")
            {
                continue;
            }

            if (field.FieldType.IsArray)
            {
                if (field.GetValue(newData) is Array array)
                {
                    for (int i = 0; i < array.Length; i++)
                    {
                        CheckOldNewValue(field.Name + '_' + i, array.GetValue(i)!);
                    }
                }
                continue;
            }

            CheckOldNewValue(field.Name, field.GetValue(newData)!);
        }
    }
    
    private void CheckOldNewValue(string propertyName, object newValue)
    {
        if (!DynDict.TryGetValue(propertyName, out object? oldValue))
        {
            oldValue = null;
        }

        if (Equals(oldValue, newValue) || !WatchedFields.Contains(propertyName))
        {
            return;
        }

        DynDict[propertyName] = newValue;

        FieldChanged?.Invoke(null, new PmdgDataFieldChangedEventArgs(propertyName, newValue));
    }
    
    //
    private void CreateEvents<T>() where T : Enum
    {
        // Map the PMDG Events to SimConnect
        foreach (T eventId in Enum.GetValues(typeof(T)))
        {
            _simConnect.MapClientEventToSimEvent(eventId, "#" + Convert.ChangeType(eventId, eventId.GetTypeCode()));
        }
    }

    private void AssociateData<T>(string clientDataName, Enum clientDataId, Enum definitionId, Enum requestId) where T : struct
    {
        // Associate an ID with the PMDG data area name
        _simConnect.MapClientDataNameToID(clientDataName, clientDataId);
        // Define the data area structure - this is a required step
        _simConnect.AddToClientDataDefinition(definitionId, 0, (uint)Marshal.SizeOf<T>(), 0, 0);
        // Register the data area structure
        _simConnect.RegisterStruct<SIMCONNECT_RECV_CLIENT_DATA, T>(definitionId);
        // Sign up for notification of data change
        _simConnect.RequestClientData(
            clientDataId,
            requestId,
            definitionId,
            SIMCONNECT_CLIENT_DATA_PERIOD.ON_SET,
            SIMCONNECT_CLIENT_DATA_REQUEST_FLAG.CHANGED,
            0,
            0,
            0);
    }

    public void RegisterB737DataAndEvents()
    {
        CreateEvents<B737.Event>();

        AssociateData<B737.Data>(B737.DataName, B737.ClientDataId.Data, B737.DefineId.Data, DataRequestId.Data);
        AssociateData<B737.Cdu>(B737.Cdu0Name, B737.ClientDataId.Cdu0, B737.DefineId.Cdu0, DataRequestId.Cdu0);
        AssociateData<B737.Cdu>(B737.Cdu1Name, B737.ClientDataId.Cdu1, B737.DefineId.Cdu1, DataRequestId.Cdu1);
    }

    private enum DataRequestId
    {
        AirPath,
        Control,
        Data,
        Cdu0,
        Cdu1,
        Cdu2
    }
}

public class PmdgDataFieldChangedEventArgs(string propertyName, object newValue) : EventArgs
{
    public string PmdgDataName { get; } = propertyName;
    public object Value { get; } = newValue;
}