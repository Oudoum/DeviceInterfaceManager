using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DeviceInterfaceManager.Converters;

public class NullableKeyValuePairConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return null;
        }

        Type valueType = value.GetType();
        if (!valueType.IsGenericType)
        {
            return null;
        }

        Type baseType = valueType.GetGenericTypeDefinition();
        if (baseType != typeof(KeyValuePair<,>) || parameter is not string stringParameter)
        {
            return null;
        }
        
        return stringParameter switch
        {
            "Key" => valueType.GetProperty("Key")?.GetValue(value, null),
            "Value" => valueType.GetProperty("Value")?.GetValue(value, null),
            _ => null
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}