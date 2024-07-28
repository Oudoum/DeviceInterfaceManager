using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DeviceInterfaceManager.Converters;

public class DoubleToIntConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int intValue)
        {
            return (double)intValue;
        }
        return 0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double doubleValue)
        {
            return null;
        }

        if (doubleValue is >= 0 and <= int.MaxValue && !double.IsNaN(doubleValue))
        {
            return (int)doubleValue;
        }
        return 0;
    }
}