using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DeviceInterfaceManager.Converters;

public class NullableConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (string.IsNullOrEmpty(value as string))
        {
            return null;
        }
        
        if (uint.TryParse(value.ToString(), out uint uValue))
        {
            return uValue;
        }
        
        return null; //Invalid input: Please provide a valid numeric value.
    }
}