using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DeviceInterfaceManager.Converters;

public class NullableCharConverter : IValueConverter
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

        return char.Parse((string)value);
    }
}