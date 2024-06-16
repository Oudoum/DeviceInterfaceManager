using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using DeviceInterfaceManager.Models;
using DeviceInterfaceManager.Models.FlightSim.MSFS.PMDG.SDK;

namespace DeviceInterfaceManager.Converters;

public class PmdgEventToStringConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is not [string eventType, int pmdgEvent])
        {
            return null;
        }

        return eventType switch
        {
            ProfileCreatorModel.Pmdg737 => Enum.GetName((B737.Event)pmdgEvent),
            ProfileCreatorModel.Pmdg777 => Enum.GetName((B777.Event)pmdgEvent),
            _ => null
        };
    }
}