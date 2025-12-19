using System;
using System.Globalization;
using Avalonia.Data.Converters;
using DeviceInterfaceManager.Models.FlightSim.MSFS.PMDG;

namespace DeviceInterfaceManager.Converters;

public class EnumToLongConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not null)
        {
            return (Mouse)value;
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}