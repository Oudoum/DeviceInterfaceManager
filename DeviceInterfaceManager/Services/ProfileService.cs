using System;
using System.Collections.Generic;
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
        outputCreator.FlightSimValue = simVar.Data.ToString(PmdgHelperService.EnglishCulture);

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
                        ProfileEntryIteration(precondition, new SimConnectClientService.SimVar(precondition.Data, Convert.ToDouble(precondition.FlightSimValue!)));
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
                     (x.PmdgData == e.PmdgDataName || (x.PmdgDataArrayIndex is not null && x.PmdgData + '_' + x.PmdgDataArrayIndex == e.PmdgDataName))))
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
                double doubleValue = Convert.ToDouble(e.Value, PmdgHelperService.EnglishCulture);
                doubleValue = Math.Round(doubleValue, 9);
                outputCreator.FlightSimValue = doubleValue.ToString(PmdgHelperService.EnglishCulture);
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

        SetDisplayValue(outputCreator, ref stringBuilder);
        SetSendOutput(outputCreator, stringBuilder);
    }

    private bool CheckPrecondition(IReadOnlyList<Precondition>? preconditions)
    {
        if (preconditions is null)
        {
            return true;
        }

        bool result = false;
        for (int i = 0; i < preconditions.Count; i++)
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
                comparisonResult = CheckComparison(matchingOutputCreator.FlightSimValue, precondition.ComparisonValue, precondition.Operator);
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
                    if (double.TryParse(outputCreator.OutputValue, out double analogValue))
                    {
                        _deviceService.SetAnalogAsync(output, analogValue);
                    }
                    break;
            }
        }
    }

    private static void SetDisplayValue(OutputCreator outputCreator, ref StringBuilder stringBuilder)
    {
        if (outputCreator.OutputType != ProfileCreatorModel.SevenSegment)
        {
            return;
        }

        if (outputCreator.DigitCount is not null)
        {
            _ = stringBuilder.Replace(".", string.Empty);
            if (stringBuilder.Length > outputCreator.DigitCount)
            {
                byte digitCount = outputCreator.DigitCount.Value;
                switch (stringBuilder[digitCount] - '0')
                {
                    case > 5:
                    {
                        stringBuilder.Length = digitCount;
                        int carry = 1;
                        for (int i = digitCount - 1; i >= 0; i--)
                        {
                            int digit = stringBuilder[i] - '0' + carry;
                            carry = digit / 10;
                            stringBuilder[i] = (char)(digit % 10 + '0');
                        }

                        if (carry > 0)
                        {
                            _ = stringBuilder.Insert(0, carry);
                        }

                        break;
                    }

                    case <= 5:
                        stringBuilder.Length = digitCount;
                        break;
                }
            }
        }

        if (outputCreator.IsPadded == true)
        {
            if (outputCreator.PaddingCharacter is not null && outputCreator.DigitCount is not null)
            {
                int dotCount = 0;
                for (int i = 0; i < stringBuilder.Length; i++)
                {
                    if (stringBuilder[i] == '.')
                    {
                        dotCount++;
                    }
                }

                while (stringBuilder.Length < outputCreator.DigitCount + dotCount) _ = stringBuilder.Insert(0, outputCreator.PaddingCharacter);
            }
        }

        if (outputCreator.DigitCount is null)
        {
            return;
        }

        _ = stringBuilder.Append('0', outputCreator.DigitCount.Value - stringBuilder.Length);
        FormatString(outputCreator, ref stringBuilder);
    }

    private static void FormatString(IOutputCreator outputCreator, ref StringBuilder stringBuilder)
    {
        if (outputCreator.DigitCheckedSum is null && outputCreator.DecimalPointCheckedSum is null)
        {
            return;
        }

        if (outputCreator.DigitCheckedSum is not null)
        {
            for (int i = 0; i < outputCreator.DigitCount; i++)
            {
                if ((outputCreator.DigitCheckedSum & (1 << i)) == 0)
                {
                    stringBuilder[i] = ' ';
                    continue;
                }

                if (stringBuilder.Length <= i)
                {
                    _ = stringBuilder.Append(outputCreator.PaddingCharacter);
                }
            }
        }

        if (outputCreator.DecimalPointCheckedSum is null)
        {
            return;
        }

        {
            byte decimalPointCount = 0;
            for (int i = 0; i < outputCreator.DigitCount + decimalPointCount; i++)
            {
                if ((outputCreator.DecimalPointCheckedSum & (1 << (i - decimalPointCount))) == 0 || stringBuilder.Length <= i)
                {
                    continue;
                }

                if (stringBuilder.Length > i + 1 && stringBuilder[i + 1] == '.')
                {
                    i++;
                    decimalPointCount++;
                    continue;
                }

                _ = stringBuilder.Insert(i + 1, '.');
                i++;
                decimalPointCount++;
            }
        }
    }

    #region Inputs

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
                //HTML Event(H:Event), Reverse Polish Notation (RPN)
                case ProfileCreatorModel.Rpn when !string.IsNullOrEmpty(inputCreator.Event) && isPressed == !inputCreator.OnRelease:
                    _simConnectClientService.SendWasmEvent(inputCreator.Event);
                    continue;

                //Key Event ID (K:Event[K]) with one parameter
                case ProfileCreatorModel.KEvent when inputCreator.Event is not null && ((isPressed && inputCreator is { DataPress: null, OnRelease: false })
                                                                                        || (!isPressed && inputCreator is { DataRelease: null, OnRelease: true })):
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

                case 0 when inputCreator.PmdgMouseRelease is not null:
                    firstParameter = (uint)inputCreator.PmdgMouseRelease.Value;
                    break;

                case 1 when inputCreator.PmdgMousePress is not null:
                    firstParameter = (uint)inputCreator.PmdgMousePress.Value;
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
            //Simulation Variable (SimVar[A]), Local Variable (L:Var[L]))
            case ProfileCreatorModel.MsfsSimConnect when inputCreator.Event is not null:
                _simConnectClientService.SetSimVar(firstParameter, inputCreator.Event);
                return;

            //Key Event ID (K:Event[K]) with one or more parameters
            case ProfileCreatorModel.KEvent when inputCreator.Event is not null:
                _simConnectClientService.TransmitSimEvent(firstParameter, secondParameter, inputCreator.Event);
                return;

            //PMDG 737
            case ProfileCreatorModel.Pmdg737 when inputCreator.PmdgEvent is not null:
                _simConnectClientService.TransmitEvent(firstParameter, (B737.Event)inputCreator.PmdgEvent.Value);
                return;

            //PMDG 777
            case ProfileCreatorModel.Pmdg777 when inputCreator.PmdgEvent is not null:
                _simConnectClientService.TransmitEvent(firstParameter, (B777.Event)inputCreator.PmdgEvent.Value);
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

            SendParameters(inputCreator, value, 0);
        }
    }

    #endregion

    public async ValueTask DisposeAsync()
    {
        await _deviceService.ResetAllOutputsAsync();

        _deviceService.AnalogValueChanged -= AnalogValueChanged;
        _deviceService.SwitchPositionChanged -= SwitchPositionChanged;
        _simConnectClientService.OnSimVarChanged -= OnOnSimVarChanged;

        GC.SuppressFinalize(this);
    }
}