using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using DeviceInterfaceManager.Models;

namespace DeviceInterfaceManager.Converters;

public class PaddingCharactersConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is char character)
        {
            return Display.PaddingCharacters.FirstOrDefault(x => x.Value == character);
        }

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is KeyValuePair<string, char> keyValuePair)
        {
            return keyValuePair.Value;
        }

        return value;
    }
}