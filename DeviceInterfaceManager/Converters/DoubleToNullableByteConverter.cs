using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DeviceInterfaceManager.Converters;

public class DoubleToNullableByteConverter :IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is byte nullableByteValue)
        {
            return (double)nullableByteValue;
        }
        return double.NaN;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double doubleValue)
        {
            return null;
        }

        if (doubleValue is >= byte.MinValue and <= byte.MaxValue && !double.IsNaN(doubleValue))
        {
            return (byte?)doubleValue;
        }
        return null;
    }
}