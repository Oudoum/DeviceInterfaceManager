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
    public event EventHandler<PmdgDataFieldChangedEventArgs>? FieldChanged;

    private B737.Data? _pmdg737Data;
    private B777.Data? _pmdg777Data;

    public List<string> WatchedFields { get; } = [];

    public IDictionary<string, object?> DynDict { get; } = new ExpandoObject();

    private bool _initialized;

    public void ReceivePmdgData(object data)
    {
        switch (data)
        {
            case B737.Data pmdg737Data:
                Iteration(pmdg737Data);
                break;

            case B777.Data pmdg777Data:
                Iteration(pmdg777Data);
                break;
        }
    }

    public void Init(SimConnect simConnect, ProfileCreatorModel profileCreatorModel)
    {
        foreach (OutputCreator output in profileCreatorModel.OutputCreators)
        {
            if (output is not { DataType: ProfileCreatorModel.Pmdg737 or ProfileCreatorModel.Pmdg777, IsActive: true })
            {
                continue;
            }

            if (string.IsNullOrEmpty(output.PmdgData))
            {
                continue;
            }

            string? pmdgDataFieldName = ConvertDataToPmdgDataFieldName(output);
            if (pmdgDataFieldName is not null && !WatchedFields.Contains(pmdgDataFieldName))
            {
                WatchedFields.Add(pmdgDataFieldName);
            }
        }
        
        if (_initialized)
        {
            return;
        }
        
        string? type = profileCreatorModel.InputCreators.FirstOrDefault(x => x.EventType is ProfileCreatorModel.Pmdg737 or ProfileCreatorModel.Pmdg777 && x.IsActive)?.EventType
                       ?? profileCreatorModel.OutputCreators.FirstOrDefault(x => x.DataType is ProfileCreatorModel.Pmdg737 or ProfileCreatorModel.Pmdg777 && x.IsActive)?.DataType;

        switch (type)
        {
            case ProfileCreatorModel.Pmdg737:
                _pmdg737Data = new B737.Data();
                Initialize(_pmdg737Data);
                RegisterB737DataAndEvents(simConnect);
                _initialized = true;
                break;

            case ProfileCreatorModel.Pmdg777:
                _pmdg777Data = new B777.Data();
                Initialize(_pmdg777Data);
                RegisterB777DataAndEvents(simConnect);
                _initialized = true;
                break;
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

    private void Initialize<T>(T? pmdgData) where T : struct
    {
        foreach (FieldInfo field in typeof(T).GetFields())
        {
            if (field.Name == "reserved")
            {
                continue;
            }

            if (field.FieldType.IsArray)
            {
                Array array = (Array)field.GetValue(pmdgData)!;
                if (field.GetCustomAttributes(typeof(MarshalAsAttribute), false).FirstOrDefault() is MarshalAsAttribute marshalAsAttribute)
                {
                    array = Array.CreateInstance(field.FieldType.GetElementType()!, marshalAsAttribute.SizeConst);
                    field.SetValue(pmdgData, array);
                }

                int i = 0;
                foreach (object item in array)
                {
                    DynDict[field.Name + '_' + i] = item;
                    i++;
                }
            }

            DynDict[field.Name] = field.GetValue(pmdgData);
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
    private static void CreateEvents<T>(SimConnect simConnect) where T : Enum
    {
        // Map the PMDG Events to SimConnect
        foreach (T eventId in Enum.GetValues(typeof(T)))
        {
            simConnect.MapClientEventToSimEvent(eventId, "#" + Convert.ChangeType(eventId, eventId.GetTypeCode()));
        }
    }

    private static void AssociateData<T>(SimConnect simConnect, string clientDataName, Enum clientDataId, Enum definitionId, Enum requestId) where T : struct
    {
        // Associate an ID with the PMDG data area name
        simConnect.MapClientDataNameToID(clientDataName, clientDataId);
        // Define the data area structure - this is a required step
        simConnect.AddToClientDataDefinition(definitionId, 0, (uint)Marshal.SizeOf<T>(), 0, 0);
        // Register the data area structure
        simConnect.RegisterStruct<SIMCONNECT_RECV_CLIENT_DATA, T>(definitionId);
        // Sign up for notification of data change
        simConnect.RequestClientData(
            clientDataId,
            requestId,
            definitionId,
            SIMCONNECT_CLIENT_DATA_PERIOD.ON_SET,
            SIMCONNECT_CLIENT_DATA_REQUEST_FLAG.CHANGED,
            0,
            0,
            0);
    }

    private static void RegisterB737DataAndEvents(SimConnect simConnect)
    {
        CreateEvents<B737.Event>(simConnect);

        AssociateData<B737.Data>(simConnect, B737.DataName, B737.ClientDataId.Data, B737.DefineId.Data, DataRequestId.Data);
        AssociateData<B737.Cdu>(simConnect, B737.Cdu0Name, B737.ClientDataId.Cdu0, B737.DefineId.Cdu0, DataRequestId.Cdu0);
        AssociateData<B737.Cdu>(simConnect, B737.Cdu1Name, B737.ClientDataId.Cdu1, B737.DefineId.Cdu1, DataRequestId.Cdu1);
    }

    private static void RegisterB777DataAndEvents(SimConnect simConnect)
    {
        CreateEvents<B777.Event>(simConnect);

        AssociateData<B777.Data>(simConnect, B777.DataName, B777.ClientDataId.Data, B777.DefineId.Data, DataRequestId.Data);
        AssociateData<B777.Cdu>(simConnect, B777.Cdu0Name, B777.ClientDataId.Cdu0, B777.DefineId.Cdu0, DataRequestId.Cdu0);
        AssociateData<B777.Cdu>(simConnect, B777.Cdu1Name, B777.ClientDataId.Cdu1, B777.DefineId.Cdu1, DataRequestId.Cdu1);
        AssociateData<B777.Cdu>(simConnect, B777.Cdu2Name, B777.ClientDataId.Cdu2, B777.DefineId.Cdu2, DataRequestId.Cdu2);
    }

    public enum DataRequestId
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