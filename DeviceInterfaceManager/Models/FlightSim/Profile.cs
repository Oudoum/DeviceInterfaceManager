using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeviceInterfaceManager.Models.Devices;
using DeviceInterfaceManager.Models.FlightSim.MSFS;
using DeviceInterfaceManager.Models.FlightSim.MSFS.PMDG.SDK;
using DeviceInterfaceManager.Models.Modifiers;

namespace DeviceInterfaceManager.Models.FlightSim;

public class Profile : IAsyncDisposable
{
    private readonly SimConnectClient _simConnectClient;

    private readonly ProfileCreatorModel _profileCreatorModel;

    private readonly IInputOutputDevice _inputOutputDevice;

    public Profile(SimConnectClient simConnectClient, ProfileCreatorModel profileCreatorModel, IInputOutputDevice inputOutputDevice)
    {
        _simConnectClient = simConnectClient;
        _profileCreatorModel = profileCreatorModel;
        _inputOutputDevice = inputOutputDevice;

        _simConnectClient.PmdgHelper.Init(profileCreatorModel);

        _simConnectClient.OnSimVarChanged += SimConnectClientOnOnSimVarChanged;

        _simConnectClient.PmdgHelper.FieldChanged += PmdgHelperOnFieldChanged;

        _inputOutputDevice.SwitchPositionChanged += SwitchPositionChanged;

        foreach (string watchedField in _simConnectClient.PmdgHelper.WatchedFields)
        {
            object? obj = _simConnectClient.PmdgHelper.DynDict[watchedField];
            if (obj is not null)
            {
                PmdgHelperOnFieldChanged(this, new PmdgDataFieldChangedEventArgs(watchedField, obj));
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
                _simConnectClient.RegisterSimVar(outputCreator.Data!, outputCreator.Unit);
                continue;
            }

            _simConnectClient.RegisterSimVar(outputCreator.Data!);
        }

        foreach (InputCreator inputCreator in _profileCreatorModel.InputCreators.Where(x =>
                     x is { IsActive: true, EventType: ProfileCreatorModel.KEvent, Event: not null }))
        {
            _simConnectClient.RegisterSimEvent(inputCreator.Event!);
        }
    }

    private void SimConnectClientOnOnSimVarChanged(object? sender, SimConnectClient.SimVar simVar)
    {
        if (simVar is { Name: "CAMERA STATE", Data: <= 6 })
        {
            foreach (Component component in _inputOutputDevice.Switch.Components)
            {
                SendEvent(component.Position, component.IsSet);
            }
        }

        foreach (OutputCreator outputCreator in _profileCreatorModel.OutputCreators.Where(x =>
                     x is { IsActive: true, DataType: ProfileCreatorModel.MsfsSimConnect } &&
                     x.Data == simVar.Name))
        {
            ProfileEntryIteration(outputCreator, simVar);
        }
    }

    private void ProfileEntryIteration(OutputCreator outputCreator, SimConnectClient.SimVar simVar)
    {
        outputCreator.FlightSimValue = simVar.Data.ToString(Helper.EnglishCulture);

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
                        ProfileEntryIteration(precondition, new SimConnectClient.SimVar(precondition.Data, Convert.ToDouble(precondition.FlightSimValue!)));
                    }

                    break;

                case ProfileCreatorModel.Pmdg737:
                case ProfileCreatorModel.Pmdg777:
                    string? propertyName = Helper.ConvertDataToPmdgDataFieldName(precondition);
                    if (!string.IsNullOrEmpty(propertyName))
                    {
                        ProfileEntryIteration(precondition, new PmdgDataFieldChangedEventArgs(propertyName, precondition.FlightSimValue!));
                    }

                    break;
            }
        }
    }

    private void PmdgHelperOnFieldChanged(object? sender, PmdgDataFieldChangedEventArgs e)
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
                double doubleValue = Convert.ToDouble(e.Value, Helper.EnglishCulture);
                doubleValue = Math.Round(doubleValue, 9);
                outputCreator.FlightSimValue = doubleValue.ToString(Helper.EnglishCulture);
                break;
        }

        ProfileIteration(outputCreator);
    }

    private void ProfileIteration(OutputCreator outputCreator)
    {
        outputCreator.OutputValue = null;
        if (!CheckPrecomparison(outputCreator.Preconditions))
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

    private bool CheckPrecomparison(IReadOnlyList<Precondition>? preconditions)
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
                    _inputOutputDevice.SetLedAsync(output, boolValue);
                    break;

                case ProfileCreatorModel.Dataline:
                    _inputOutputDevice.SetDatalineAsync(output, boolValue);
                    break;

                case ProfileCreatorModel.SevenSegment:
                    _inputOutputDevice.SetSevenSegmentAsync(output, outputCreator.OutputValue);
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
        foreach (InputCreator inputCreator in _profileCreatorModel.InputCreators.Where(x => x.IsActive && x.Input?.Position == position))
        {
            //Precondition
            if (!CheckPrecomparison(inputCreator.Preconditions))
            {
                continue;
            }

            switch (inputCreator.EventType)
            {
                //HTML Event(H:Event), Reverse Polish Notation (RPN)
                case ProfileCreatorModel.Rpn when !string.IsNullOrEmpty(inputCreator.Event) && isPressed == !inputCreator.OnRelease:
                    _simConnectClient.SendWasmEvent(inputCreator.Event);
                    continue;

                //Key Event ID (K:Event[K]) with one parameter
                case ProfileCreatorModel.KEvent when inputCreator.Event is not null && ((isPressed && inputCreator is { DataPress: null, OnRelease: false })
                                                                                        || (!isPressed && inputCreator is { DataRelease: null, OnRelease: true })):
                    _simConnectClient.TransmitSimEvent(inputCreator.Event);
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

            switch (inputCreator.EventType)
            {
                //Simulation Variable (SimVar[A]), Local Variable (L:Var[L]))
                case ProfileCreatorModel.MsfsSimConnect when inputCreator.Event is not null:
                    _simConnectClient.SetSimVar(firstParameter, inputCreator.Event);
                    continue;

                //Key Event ID (K:Event[K]) with one or more parameters
                case ProfileCreatorModel.KEvent when inputCreator.Event is not null && (inputCreator.DataPress is not null || inputCreator.DataRelease is not null):
                    _simConnectClient.TransmitSimEvent(firstParameter, secondParameter, inputCreator.Event);
                    continue;

                //PMDG 737
                case ProfileCreatorModel.Pmdg737 when inputCreator.PmdgEvent is not null:
                    _simConnectClient.TransmitEvent(firstParameter, (B737.Event)inputCreator.PmdgEvent.Value);
                    continue;

                //PMDG 777
                case ProfileCreatorModel.Pmdg777 when inputCreator.PmdgEvent is not null:
                    _simConnectClient.TransmitEvent(firstParameter, (B777.Event)inputCreator.PmdgEvent.Value);
                    continue;
            }
        }
    }

    #endregion

    public async ValueTask DisposeAsync()
    {
        await _inputOutputDevice.ResetAllOutputsAsync();

        _inputOutputDevice.SwitchPositionChanged -= SwitchPositionChanged;
        _simConnectClient.OnSimVarChanged -= SimConnectClientOnOnSimVarChanged;

        GC.SuppressFinalize(this);
    }
}