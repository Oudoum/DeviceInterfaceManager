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

        if (long.TryParse(value.ToString(), out long longValue))
        {
            return longValue;
        }

        return null; //Invalid input: Please provide a valid numeric value.
    }
}