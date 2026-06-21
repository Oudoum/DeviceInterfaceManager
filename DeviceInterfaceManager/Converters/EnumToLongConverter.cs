using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using DeviceInterfaceManager.Models.FlightSim.MSFS.PMDG;

namespace DeviceInterfaceManager.Converters;

public class EnumToLongConverter : IValueConverter, IMultiValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not null && Enum.IsDefined(typeof(Mouse), value))
        {
            return (Mouse)value;
        }

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 2)
        {
            return null;
        }

        object? value1 = values[0];
        object? value2 = values[1];
        if (value2 is UnsetValueType)
        {
            return null;
        }

        if (value1 is string str && str.StartsWith("PMDG") && value2 is not null && Enum.IsDefined(typeof(Mouse), value2))
        {
            return (Mouse)value2;
        }

        return value2;
    }
}