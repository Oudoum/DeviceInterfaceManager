using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeviceInterfaceManager.Models.Devices;
using DeviceInterfaceManager.Models.FlightSim.MSFS;
using DeviceInterfaceManager.Models.FlightSim.MSFS.PMDG.SDK;

namespace DeviceInterfaceManager.Models.FlightSim;

public class Profile : IAsyncDisposable
{
    private readonly SimConnectClient _simConnectClient;

    private readonly ProfileCreatorModel _profileCreatorModel;

    private readonly IInputOutputDevice _inputOutputDevice;

    private const double Tolerance = 0.000001;

    public Profile(SimConnectClient simConnectClient, ProfileCreatorModel profileCreatorModel, IInputOutputDevice inputOutputDevice)
    {
        _simConnectClient = simConnectClient;
        _profileCreatorModel = profileCreatorModel;
        _inputOutputDevice = inputOutputDevice;
        
        if (_simConnectClient.SimConnect is null)
        {
            return;
        }
        
        _simConnectClient.PmdgHelper.Init(_simConnectClient.SimConnect, profileCreatorModel);

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
                     x.DataType == ProfileCreatorModel.MsfsSimConnect &&
                     !string.IsNullOrEmpty(x.Data) &&
                     x.IsActive))
        {
            if (!string.IsNullOrEmpty(outputCreator.Unit))
            {
                _simConnectClient.RegisterSimVar(outputCreator.Data!, outputCreator.Unit);
                continue;
            }

            _simConnectClient.RegisterSimVar(outputCreator.Data!);
        }

        foreach (InputCreator inputCreator in _profileCreatorModel.InputCreators.Where(x =>
                     x.EventType == ProfileCreatorModel.KEvent &&
                     !string.IsNullOrEmpty(x.Event) &&
                     x.IsActive))
        {
            _simConnectClient.RegisterSimEvent(inputCreator.Event!);
        }
    }

    private void SimConnectClientOnOnSimVarChanged(object? sender, SimConnectClient.SimVar simVar)
    {
        foreach (OutputCreator outputCreator in _profileCreatorModel.OutputCreators.Where(x =>
                     x.DataType == ProfileCreatorModel.MsfsSimConnect &&
                     x.Data == simVar.Name &&
                     x.IsActive))
        {
            ProfileEntryIteration(outputCreator, simVar);
            PrecomparisonIteration(outputCreator);
        }
    }

    private void ProfileEntryIteration(OutputCreator outputCreator, SimConnectClient.SimVar simVar)
    {
        outputCreator.FlightSimValue = simVar.Data;
        outputCreator.OutputValue = null;
        if (!CheckPrecomparison(outputCreator.Preconditions))
        {
            return;
        }

        switch (outputCreator.OutputType)
        {
            case ProfileCreatorModel.Led or ProfileCreatorModel.Dataline when outputCreator.ComparisonValue is null:
                SetSendOutput(outputCreator, Convert.ToBoolean(simVar.Data));
                break;

            case ProfileCreatorModel.Led or ProfileCreatorModel.Dataline:
                SetSendOutput(outputCreator, CheckComparison(outputCreator.ComparisonValue, outputCreator.Operator, simVar.Data));
                break;

            case ProfileCreatorModel.SevenSegment when outputCreator.ComparisonValue is null:
                try
                {
                    SetStringValue(simVar.Data.ToString(outputCreator.NumericFormat, CultureInfo.InvariantCulture), simVar.Name, outputCreator);
                }
                catch (Exception)
                {
                    SetNumericFormatError(outputCreator);
                }

                break;

            case ProfileCreatorModel.SevenSegment:
                SetSendOutput(outputCreator, CheckComparison(outputCreator.ComparisonValue, outputCreator.Operator, simVar.Data));
                break;
        }
    }

    private static void SetNumericFormatError(OutputCreator outputCreator)
    {
        outputCreator.OutputValue = "NumericFormat Error";
    }

    private void PrecomparisonIteration(OutputCreator outputCreator)
    {
        foreach (OutputCreator precondition in _profileCreatorModel.OutputCreators.Where(x =>
                     x.FlightSimValue is not null &&
                     x.Preconditions is not null &&
                     x.Preconditions.Length > 0 &&
                     x.IsActive &&
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
        foreach (OutputCreator item in _profileCreatorModel.OutputCreators.Where(x =>
                     (x is { DataType: ProfileCreatorModel.Pmdg737 or ProfileCreatorModel.Pmdg777, IsActive: true, PmdgDataArrayIndex: not null } &&
                      x.PmdgData + '_' + x.PmdgDataArrayIndex == e.PmdgDataName) ||
                     x.PmdgData == e.PmdgDataName))
        {
            ProfileEntryIteration(item, e);
            PrecomparisonIteration(item);
        }
    }

    private void ProfileEntryIteration(OutputCreator outputCreator, PmdgDataFieldChangedEventArgs e)
    {
        outputCreator.FlightSimValue = e.Value;
        outputCreator.OutputValue = null;
        if (!CheckPrecomparison(outputCreator.Preconditions))
        {
            return;
        }

        switch (e.Value)
        {
            //bool
            case bool valueBool:
                if (outputCreator.ComparisonValue is not null)
                {
                    SetSendOutput(outputCreator, CheckComparison(outputCreator.ComparisonValue, outputCreator.Operator, valueBool));
                    break;
                }

                SetSendOutput(outputCreator, valueBool);
                break;

            //string
            case string valueString:
                if (outputCreator.ComparisonValue is not null)
                {
                    SetSendOutput(outputCreator, CheckComparison(outputCreator.ComparisonValue, outputCreator.Operator, valueString));
                    break;
                }

                SetStringValue(valueString, e.PmdgDataName, outputCreator);
                break;

            //float
            case float:
                double valueDouble = Convert.ToDouble(e.Value);
                if (outputCreator.ComparisonValue is not null)
                {
                    SetSendOutput(outputCreator, CheckComparison(outputCreator.ComparisonValue, outputCreator.Operator, valueDouble));
                    break;
                }

                try
                {
                    SetStringValue(valueDouble.ToString(outputCreator.NumericFormat, CultureInfo.InvariantCulture), e.PmdgDataName, outputCreator);
                }
                catch (Exception)
                {
                    SetNumericFormatError(outputCreator);
                }

                break;

            //byte, ushort, short, uint, int
            default:
                if (outputCreator.ComparisonValue is not null)
                {
                    SetSendOutput(outputCreator, CheckComparison(outputCreator.ComparisonValue, outputCreator.Operator, Convert.ToDouble(e.Value)));
                    break;
                }

                long valueLong = Convert.ToInt64(e.Value);
                try
                {
                    SetStringValue(valueLong.ToString(outputCreator.NumericFormat, CultureInfo.InvariantCulture), e.PmdgDataName, outputCreator);
                }
                catch (Exception)
                {
                    SetNumericFormatError(outputCreator);
                }

                break;
        }
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
                comparisonResult = CheckComparison(precondition.ComparisonValue, precondition.Operator, matchingOutputCreator.FlightSimValue);
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

    private static bool CheckComparison(string? comparisonValue, char? charOperator, object? value)
    {
        string? sComparisonValue = comparisonValue switch
        {
            not null when comparisonValue.Equals("true", System.StringComparison.CurrentCultureIgnoreCase) => "1",
            not null when comparisonValue.Equals("false", System.StringComparison.CurrentCultureIgnoreCase) => "0",
            _ => comparisonValue
        };
        
        string? sValue = value switch
        {
            null => string.Empty,
            bool boolValue => boolValue ? "1" : "0",
            _ => value.ToString()
        };

        sValue = sValue switch
        {
            not null when sValue.Equals("true", System.StringComparison.CurrentCultureIgnoreCase) => "1",
            not null when sValue.Equals("false", System.StringComparison.CurrentCultureIgnoreCase) => "0",
            _ => sValue
        };
        
        return CheckComparison(sComparisonValue, charOperator, sValue);
    }

    private static bool CheckComparison(string? sComparisonValue, char? charOperator, string? sValue)
    {
        if (double.TryParse(sComparisonValue, out double comparisonValue) && double.TryParse(sValue, out double value))
        {
            return NumericComparison(comparisonValue, charOperator, value);
        }

        return StringComparison(sComparisonValue, charOperator, sValue);
    }

    private static bool NumericComparison(double comparisonValue, char? charOperator, double value)
    {
        return charOperator switch
        {
            '=' => Math.Abs(value - comparisonValue) < Tolerance,
            '≠' => Math.Abs(value - comparisonValue) > Tolerance,
            '<' => value < comparisonValue,
            '>' => value > comparisonValue,
            '≤' => value <= comparisonValue,
            '≥' => value >= comparisonValue,
            _ => false
        };
    }

    private static bool StringComparison(string? sComparisonValue, char? charOperator, string? sValue)
    {
        return charOperator switch
        {
            '=' => sValue == sComparisonValue,
            '≠' => sValue != sComparisonValue,
            _ => false
        };
    }

    private void SetSendOutput(OutputCreator outputCreator, bool valueBool)
    {
        valueBool = outputCreator.IsInverted ? !valueBool : valueBool;
        if (outputCreator.Output is not null)
        {
            switch (outputCreator.OutputType)
            {
                case ProfileCreatorModel.Led:
                    _inputOutputDevice.SetLedAsync(outputCreator.Output.Position.ToString(), valueBool);
                    break;

                case ProfileCreatorModel.Dataline:
                    _inputOutputDevice.SetDatalineAsync(outputCreator.Output.Position.ToString(), valueBool);
                    break;
            }
        }

        outputCreator.OutputValue = valueBool;
    }

    private void SetStringValue(string value, string name, OutputCreator outputCreator)
    {
        StringBuilder sb = new(value);
        if (outputCreator.DigitCount is not null)
        {
            _ = sb.Replace(".", string.Empty);
            if (sb.Length > outputCreator.DigitCount)
            {
                byte digitCount = outputCreator.DigitCount.Value;
                switch (sb[digitCount] - '0')
                {
                    case > 5:
                    {
                        sb.Length = digitCount;
                        int carry = 1;
                        for (int i = digitCount - 1; i >= 0; i--)
                        {
                            int digit = sb[i] - '0' + carry;
                            carry = digit / 10;
                            sb[i] = (char)(digit % 10 + '0');
                        }

                        if (carry > 0)
                        {
                            _ = sb.Insert(0, carry);
                        }

                        break;
                    }

                    case <= 5:
                        sb.Length = digitCount;
                        break;
                }
            }
        }

        if (outputCreator.IsPadded == true)
        {
            if (outputCreator.PaddingCharacter is not null && outputCreator.DigitCount is not null)
            {
                int dotCount = 0;
                for (int i = 0; i < sb.Length; i++)
                {
                    if (sb[i] == '.')
                    {
                        dotCount++;
                    }
                }

                while (sb.Length < outputCreator.DigitCount + dotCount) _ = sb.Insert(0, outputCreator.PaddingCharacter);
            }
            else if (outputCreator is { DataType: ProfileCreatorModel.Pmdg737, PaddingCharacter: null, DigitCount: null })
            {
                B737.SetMcp(ref sb, name);
                B737.SetIrsDisplay(ref sb, name);
            }
        }

        if (outputCreator.DigitCount is not null)
        {
            _ = sb.Append('0', outputCreator.DigitCount.Value - sb.Length);
            FormatString(ref sb, outputCreator);
        }

        if (outputCreator.SubstringStart is null && outputCreator.SubstringEnd is not null)
        {
            sb.Length = outputCreator.SubstringEnd is not null ? (int)(outputCreator.SubstringEnd + 1) : sb.Length;
        }
        else if (outputCreator.SubstringStart is not null && outputCreator.SubstringStart <= sb.Length - 1)
        {
            _ = sb.Remove(0, (int)outputCreator.SubstringStart);
            if (outputCreator.SubstringEnd is not null)
            {
                sb.Length = (int)(outputCreator.SubstringEnd - outputCreator.SubstringStart + 1);
            }
        }

        string outputValue = sb.ToString();
        if (outputCreator.Output is not null)
        {
            _inputOutputDevice.SetSevenSegmentAsync(outputCreator.Output.Position.ToString(), outputValue);
        }

        outputCreator.OutputValue = outputValue;
    }

    private static void FormatString(ref StringBuilder sb, IOutputCreator outputCreator)
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
                    sb[i] = ' ';
                    continue;
                }

                if (sb.Length <= i)
                {
                    _ = sb.Append(outputCreator.PaddingCharacter);
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
                if ((outputCreator.DecimalPointCheckedSum & (1 << (i - decimalPointCount))) == 0 || sb.Length <= i)
                {
                    continue;
                }

                if (sb.Length > i + 1 && sb[i + 1] == '.')
                {
                    i++;
                    decimalPointCount++;
                    continue;
                }

                _ = sb.Insert(i + 1, '.');
                i++;
                decimalPointCount++;
            }
        }
    }

    private void SwitchPositionChanged(object? sender, SwitchPositionChangedEventArgs e)
    {
        foreach (InputCreator inputCreator in _profileCreatorModel.InputCreators.Where(x => x.Input?.Position == e.Position && x.IsActive))
        {
            //Precondition
            if (inputCreator.Preconditions is null && !CheckPrecomparison(inputCreator.Preconditions))
            {
                continue;
            }

            //HTML Event(H:Event), Reverse Polish Notation (RPN)
            if (inputCreator.EventType == ProfileCreatorModel.Rpn && !string.IsNullOrEmpty(inputCreator.Event) && e.IsPressed == !inputCreator.OnRelease)
            {
                _simConnectClient.SendWasmEvent(inputCreator.Event);
                continue;
            }

            //Direction
            uint firstParameter = Convert.ToUInt32(e.IsPressed);
            uint secondParameter = 0;
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
                //Simulation Variable (SimVar[A]), Local Variable (L:Var[L]), Key Event ID (K:Event[K])
                case ProfileCreatorModel.MsfsSimConnect when !string.IsNullOrEmpty(inputCreator.Event):
                    _simConnectClient.SetSimVar(firstParameter, inputCreator.Event);
                    continue;

                case ProfileCreatorModel.KEvent when !string.IsNullOrEmpty(inputCreator.Event):
                    _simConnectClient.TransmitSimEvent(firstParameter, secondParameter, inputCreator.Event);
                    continue;

                //PMDG 737
                case ProfileCreatorModel.Pmdg737 when inputCreator.PmdgEvent is not null:
                    _simConnectClient.TransmitEvent(firstParameter, (B737.Event)inputCreator.PmdgEvent.Value);
                    break;

                //PMDG 777
                case ProfileCreatorModel.Pmdg777 when inputCreator.PmdgEvent is not null:
                    _simConnectClient.TransmitEvent(firstParameter, (B777.Event)inputCreator.PmdgEvent.Value);
                    break;
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _inputOutputDevice.ResetAllOutputsAsync();

        _inputOutputDevice.SwitchPositionChanged -= SwitchPositionChanged;
        _simConnectClient.OnSimVarChanged -= SimConnectClientOnOnSimVarChanged;

        GC.SuppressFinalize(this);
    }
}