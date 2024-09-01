using System;
using System.Globalization;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DeviceInterfaceManager.Models.Modifiers;

public partial class Comparison : ObservableObject, IModifier
{
    [ObservableProperty]
    private bool _isActive = true;

    [ObservableProperty]
    private char _operator = Equal;

    public static char[] Operators => [Equal, NotEqual, GreaterThan, LessThan, GreaterThanOrEqual, LessThanOrEqual];

    private const char Equal = '=';
    private const char NotEqual = '≠';
    private const char GreaterThan = '>';
    private const char LessThan = '<';
    private const char GreaterThanOrEqual = '≥';
    private const char LessThanOrEqual = '≤';

    [ObservableProperty]
    private string _value = string.Empty;

    [ObservableProperty]
    private string _trueValue = string.Empty;

    [ObservableProperty]
    private string _falseValue = string.Empty;

    private const double Tolerance = 0.000001;

    public void Apply(ref StringBuilder value)
    {
        string sValue = value.ToString();
        bool comparison = false;
        bool isDouble;
        if ((isDouble = double.TryParse(sValue, CultureInfo.InvariantCulture, out double simValue)) && (isDouble = double.TryParse(Value, CultureInfo.InvariantCulture, out double comparisonValue)))
        {
            comparison = CheckComparison(simValue, comparisonValue, Operator);
        }

        if (!isDouble)
        {
            comparison = CheckComparison(sValue, Value, Operator);
        }

        switch (comparison)
        {
            case true when !string.IsNullOrEmpty(TrueValue):
                value = new StringBuilder(TrueValue);
                break;

            case true:
                break;

            case false when !string.IsNullOrEmpty(FalseValue):
                value = new StringBuilder(FalseValue);
                break;

            case false:
                value = new StringBuilder();
                break;
        }
    }

    public static bool CheckComparison(double simValue, double comparisonValue, char? charOperator)
    {
        return charOperator switch
        {
            Equal => Math.Abs(simValue - comparisonValue) < Tolerance,
            NotEqual => Math.Abs(simValue - comparisonValue) > Tolerance,
            GreaterThan => simValue > comparisonValue,
            LessThan => simValue < comparisonValue,
            GreaterThanOrEqual => simValue >= comparisonValue,
            LessThanOrEqual => simValue <= comparisonValue,
            _ => false
        };
    }

    public static bool CheckComparison(string? simValue, string? comparisonValue, char? charOperator)
    {
        return charOperator switch
        {
            Equal => simValue == comparisonValue,
            NotEqual => simValue != comparisonValue,
            _ => false
        };
    }
    
    public object Clone()
    {
        return MemberwiseClone();
    }
}