using System;
using System.Globalization;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using NCalc;

namespace DeviceInterfaceManager.Models.Modifiers;

public partial class Transformation : ObservableObject, IModifier
{
    [ObservableProperty]
    private bool _isActive = true;

    [ObservableProperty]
    private string _expression = "$";
    
    public void Apply(ref StringBuilder value)
    {
        string expression = Expression.Replace("$", value.ToString());
        Expression calcExpression = new(expression);

        try
        {
            double? result = (double?)calcExpression.Evaluate();

            if (result is null)
            {
                return;
            }
            
            value = new StringBuilder(result.Value.ToString(CultureInfo.InvariantCulture));
        }
        catch (Exception)
        {
            value = new StringBuilder("E");
        }
    }
    
    public object Clone()
    {
        return MemberwiseClone();
    }
}