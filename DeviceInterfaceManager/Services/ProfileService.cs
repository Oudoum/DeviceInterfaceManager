using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeviceInterfaceManager.Models;
using DeviceInterfaceManager.Models.Devices;
using DeviceInterfaceManager.Models.FlightSim.MSFS.PMDG;
using DeviceInterfaceManager.Models.Modifiers;
using DeviceInterfaceManager.Services.Devices;

namespace DeviceInterfaceManager.Services;

public class ProfileService : IAsyncDisposable
{
    private readonly SimConnectClientService _simConnectClientService;
    private readonly ProfileCreatorModel _profileCreatorModel;
    private readonly IDeviceService _deviceService;

    public ProfileService(SimConnectClientService simConnectClientService, PmdgHelperService pmdgHelperService, ProfileCreatorModel profileCreatorModel, IDeviceService deviceService)
    {
        _simConnectClientService = simConnectClientService;
        _profileCreatorModel = profileCreatorModel;
        _deviceService = deviceService;

        pmdgHelperService.InitializeProfile(profileCreatorModel);

        _simConnectClientService.OnSimVarChanged += OnOnSimVarChanged;

        pmdgHelperService.FieldChanged += PmdgPmdgHelperServiceOnFieldChanged;

        _deviceService.SwitchPositionChanged += SwitchPositionChanged;
        _deviceService.AnalogValueChanged += AnalogValueChanged;

        foreach (string watchedField in pmdgHelperService.WatchedFields)
        {
            if (!pmdgHelperService.DynDict.TryGetValue(watchedField, out object? obj))
            {
                continue;
            }

            if (obj is not null)
            {
                PmdgPmdgHelperServiceOnFieldChanged(this, new PmdgDataFieldChangedEventArgs(watchedField, obj));
            }
        }

        foreach (OutputCreator outputCreator in _profileCreatorModel.OutputCreators.Where(x =>
                     x is { IsActive: true, DataType: ProfileCreatorModel.MsfsSimConnect, Data: not null } || x.DataType == ProfileCreatorModel.Dim))
        {
            if (outputCreator.DataType == ProfileCreatorModel.Dim)
            {
                outputCreator.FlightSimValue = "1";
                ProfileIteration(outputCreator);
                continue;
            }

            if (!string.IsNullOrEmpty(outputCreator.Unit))
            {
                _simConnectClientService.RegisterSimVar(outputCreator.Data!, outputCreator.Unit);
                continue;
            }

            _simConnectClientService.RegisterSimVar(outputCreator.Data!);
        }

        foreach (InputCreator inputCreator in _profileCreatorModel.InputCreators.Where(x =>
                     x is { IsActive: true, EventType: ProfileCreatorModel.KEvent, Event: not null }))
        {
            _simConnectClientService.RegisterSimEvent(inputCreator.Event!);
        }
    }

    private void OnOnSimVarChanged(object? sender, SimConnectClientService.SimVar simVar)
    {
        if (simVar is { Name: "CAMERA STATE", Data: <= 6 })
        {
            if (_deviceService.Inputs is not null)
            {
                foreach (Component component in _deviceService.Inputs.Switch.Components)
                {
                    SendEvent(component.Position, component.IsSet);
                }
                
            }
        }

        foreach (OutputCreator outputCreator in _profileCreatorModel.OutputCreators.Where(x =>
                     x is { IsActive: true, DataType: ProfileCreatorModel.MsfsSimConnect } &&
                     x.Data == simVar.Name))
        {
            ProfileEntryIteration(outputCreator, simVar);
        }
    }

    private void ProfileEntryIteration(OutputCreator outputCreator, SimConnectClientService.SimVar simVar)
    {
        outputCreator.FlightSimValue = simVar.Data.ToString(CultureInfo.InvariantCulture);

        ProfileIteration(outputCreator);
    }

    private void PreconditionIteration(OutputCreator outputCreator)
    {
        foreach (OutputCreator precondition in _profileCreatorModel.OutputCreators.Where(x =>
                     x is { IsActive: true, FlightSimValue: not null, Preconditions.Length: > 0 } &&
                     x.Preconditions.Any(l => l.ReferenceId == outputCreator.Id)))
        {
            switch (precondition.DataType)
            {
                case ProfileCreatorModel.MsfsSimConnect:
                    if (precondition.Data is not null)
                    {
                        ProfileEntryIteration(precondition, new SimConnectClientService.SimVar(precondition.Data, Convert.ToDouble(precondition.FlightSimValue, CultureInfo.InvariantCulture)));
                    }

                    break;

                case ProfileCreatorModel.Pmdg737:
                case ProfileCreatorModel.Pmdg777:
                    string? propertyName = PmdgHelperService.ConvertDataToPmdgDataFieldName(precondition);
                    if (!string.IsNullOrEmpty(propertyName))
                    {
                        ProfileEntryIteration(precondition, new PmdgDataFieldChangedEventArgs(propertyName, precondition.FlightSimValue!));
                    }

                    break;
            }
        }
    }

    private void PmdgPmdgHelperServiceOnFieldChanged(object? sender, PmdgDataFieldChangedEventArgs e)
    {
        foreach (OutputCreator outputCreator in _profileCreatorModel.OutputCreators.Where(x =>
                     x is { IsActive: true, DataType: ProfileCreatorModel.Pmdg737 or ProfileCreatorModel.Pmdg777 } &&
                     (x.Data == e.PmdgDataName || (!string.IsNullOrEmpty(x.Unit) && x.Data + '_' + x.Unit == e.PmdgDataName))))
        {
            ProfileEntryIteration(outputCreator, e);
        }
    }

    private void ProfileEntryIteration(OutputCreator outputCreator, PmdgDataFieldChangedEventArgs e)
    {
        switch (e.Value)
        {
            case string eValue:
                outputCreator.FlightSimValue = eValue;
                break;

            //bool, byte, ushort, short, uint, int, float
            default:
                double doubleValue = Convert.ToDouble(e.Value, CultureInfo.InvariantCulture);
                doubleValue = Math.Round(doubleValue, 9);
                outputCreator.FlightSimValue = doubleValue.ToString(CultureInfo.InvariantCulture);
                break;
        }

        ProfileIteration(outputCreator);
    }

    private void ProfileIteration(OutputCreator outputCreator)
    {
        outputCreator.OutputValue = null;
        if (!CheckPrecondition(outputCreator.Preconditions))
        {
            return;
        }

        PreconditionIteration(outputCreator);

        if (outputCreator.OutputType is null)
        {
            return;
        }

        if (outputCreator.FlightSimValue is null)
        {
            return;
        }

        StringBuilder stringBuilder = new(outputCreator.FlightSimValue);
        if (outputCreator.Modifiers is not null)
        {
            foreach (IModifier modifier in outputCreator.Modifiers)
            {
                if (modifier.IsActive)
                {
                    modifier.Apply(ref stringBuilder);
                }
            }
        }

        outputCreator.Display?.SetDisplayValue(ref stringBuilder);
        SetSendOutput(outputCreator, stringBuilder);
    }

    private bool CheckPrecondition(Precondition[]? preconditions)
    {
        if (preconditions is null)
        {
            return true;
        }

        bool result = false;
        for (int i = 0; i < preconditions.Length; i++)
        {
            Precondition precondition = preconditions[i];
            OutputCreator? matchingOutputCreator = _profileCreatorModel.OutputCreators.FirstOrDefault(oc => oc.Id == precondition.ReferenceId);
            if (matchingOutputCreator is null)
            {
                return false;
            }

            bool comparisonResult = true;
            if (precondition.IsActive)
            {
                comparisonResult = CheckComparison(precondition.UseOutputValue ? matchingOutputCreator.OutputValue : matchingOutputCreator.FlightSimValue, precondition.ComparisonValue, precondition.Operator);
            }

            if (i == 0)
            {
                result = comparisonResult;
                continue;
            }

            if (preconditions[i - 1].IsOrOperator)
            {
                result = result || comparisonResult;
                continue;
            }

            result = result && comparisonResult;
        }

        return result;
    }

    private static bool CheckComparison(string? sSimValue, string? sComparisonValue, char? charOperator)
    {
        if (double.TryParse(sSimValue, CultureInfo.InvariantCulture, out double simValue) && double.TryParse(sComparisonValue, CultureInfo.InvariantCulture, out double comparisonValue))
        {
            return Comparison.CheckComparison(simValue, comparisonValue, charOperator);
        }

        return Comparison.CheckComparison(sSimValue, sComparisonValue, charOperator);
    }

    private void SetSendOutput(OutputCreator outputCreator, StringBuilder stringBuilder)
    {
        outputCreator.OutputValue = stringBuilder.ToString();
        if (outputCreator.Outputs is null || string.IsNullOrEmpty(outputCreator.OutputValue))
        {
            return;
        }

        bool boolValue = outputCreator.OutputValue != "0";

        foreach (int output in outputCreator.Outputs)
        {
            switch (outputCreator.OutputType)
            {
                case ProfileCreatorModel.Led:
                    _deviceService.SetLedAsync(output, boolValue);
                    break;

                case ProfileCreatorModel.Dataline:
                    _deviceService.SetDatalineAsync(output, boolValue);
                    break;

                case ProfileCreatorModel.SevenSegment:
                    _deviceService.SetSevenSegmentAsync(output, outputCreator.OutputValue);
                    break;
                
                case ProfileCreatorModel.Analog:
                    if (double.TryParse(outputCreator.OutputValue, CultureInfo.InvariantCulture, out double analogValue))
                    {
                        _deviceService.SetAnalogAsync(output, analogValue);
                    }
                    break;
            }
        }
    }

    private void SwitchPositionChanged(object? sender, SwitchPositionChangedEventArgs e)
    {
        SendEvent(e.Position, e.IsPressed);
    }

    private void SendEvent(int position, bool isPressed)
    {
        foreach (InputCreator inputCreator in _profileCreatorModel.InputCreators.Where(x => x is { IsActive: true, InputType: ProfileCreatorModel.Switch } && x.Input == position))
        {
            //Precondition
            if (!CheckPrecondition(inputCreator.Preconditions))
            {
                continue;
            }

            switch (inputCreator.EventType)
            {
                //HTML Event [H], Reverse Polish Notation (RPN)
                case ProfileCreatorModel.Rpn when !string.IsNullOrEmpty(inputCreator.Event) && ((isPressed && inputCreator.DataPress is null)
                                                                                                || (!isPressed && inputCreator.DataRelease is not null)):
                    _simConnectClientService.SendWasmEvent(inputCreator.Event);
                    continue;

                //Key Event ID [K] with one parameter
                case ProfileCreatorModel.KEvent when !string.IsNullOrEmpty(inputCreator.Event) && ((isPressed && inputCreator.DataPress is not null)
                                                                                                   || (!isPressed && inputCreator.DataRelease is not null)):
                    _simConnectClientService.TransmitSimEvent(inputCreator.Event);
                    continue;
            }

            //Direction
            long firstParameter = Convert.ToInt32(isPressed);
            long secondParameter = 0;
            switch (firstParameter)
            {
                case 0 when inputCreator.DataRelease is not null:
                    firstParameter = inputCreator.DataRelease.Value;
                    if (inputCreator.DataRelease2 is not null)
                    {
                        secondParameter = inputCreator.DataRelease2.Value;
                    }

                    break;

                case 1 when inputCreator.DataPress is not null:
                    firstParameter = inputCreator.DataPress.Value;
                    if (inputCreator.DataPress2 is not null)
                    {
                        secondParameter = inputCreator.DataPress2.Value;
                    }

                    break;

                default:
                    continue;
            }

            SendParameters(inputCreator, firstParameter, secondParameter);
        }
    }

    private void SendParameters(InputCreator inputCreator, double firstParameter, double secondParameter)
    {
        switch (inputCreator.EventType)
        {
            //Simulation Variable [A] and Local Variable [L]
            case ProfileCreatorModel.MsfsSimConnect when inputCreator.Event is not null:
                _simConnectClientService.SetSimVar(firstParameter, inputCreator.Event);
                return;

            //Key Event ID [K] with one or more parameters
            case ProfileCreatorModel.KEvent when inputCreator.Event is not null:
                _simConnectClientService.TransmitSimEvent(firstParameter, secondParameter, inputCreator.Event);
                return;

            //PMDG 737
            case ProfileCreatorModel.Pmdg737 when inputCreator.Event is not null:
                if (Enum.TryParse(inputCreator.Event, out B737.Event b737Event))
                {
                    _simConnectClientService.TransmitEvent(firstParameter, b737Event);
                }

                return;

            //PMDG 777
            case ProfileCreatorModel.Pmdg777 when inputCreator.Event is not null:
                if (Enum.TryParse(inputCreator.Event, out B777.Event b777Event))
                {
                    _simConnectClientService.TransmitEvent(firstParameter, b777Event);
                }

                return;
        }
    }

    private void AnalogValueChanged(object? sender, AnalogValueChangedEventArgs e)
    {
        foreach (InputCreator inputCreator in _profileCreatorModel.InputCreators.Where(x => x is { IsActive: true, InputType: ProfileCreatorModel.Analog } && x.Input == e.Position))
        {
            //Precondition
            if (!CheckPrecondition(inputCreator.Preconditions))
            {
                continue;
            }
            
            double value = e.Value;

            if (inputCreator.Interpolation is not null)
            {
                StringBuilder stringBuilder = new(e.Value.ToString(CultureInfo.InvariantCulture));
                inputCreator.Interpolation.Apply(ref stringBuilder);
                try
                {
                    string sValue = stringBuilder.ToString();
                    value = Convert.ToDouble(sValue, CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    //
                }
            }

            if (inputCreator.DataPress is not null)
            {
                SendParameters(inputCreator, inputCreator.DataPress.Value, value);
                continue;
            }

            SendParameters(inputCreator, value, 0);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _deviceService.ResetAllOutputsAsync();

        _deviceService.AnalogValueChanged -= AnalogValueChanged;
        _deviceService.SwitchPositionChanged -= SwitchPositionChanged;
        _simConnectClientService.OnSimVarChanged -= OnOnSimVarChanged;

        GC.SuppressFinalize(this);
    }
}