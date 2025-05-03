using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Data.Converters;
using DeviceInterfaceManager.Models;
using DeviceInterfaceManager.Models.FlightSim.MSFS.PMDG;

namespace DeviceInterfaceManager.Converters;

public static class FuncConverters
{
    public static FuncMultiValueConverter<object?, string?> PmdgEventToStringMultiConverter { get; } = new(GetPmdgEventString);

    private static string? GetPmdgEventString(IEnumerable<object?> events)
    {
        object?[] eventsList = events.ToArray();
        if (eventsList is not [string eventType, int pmdgEvent])
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