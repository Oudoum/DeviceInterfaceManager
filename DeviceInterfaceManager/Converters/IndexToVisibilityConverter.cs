using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using DeviceInterfaceManager.Models.Modifiers;

namespace DeviceInterfaceManager.Converters;

public class IndexToVisibilityConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values[0] is not Interpolation.InterpolationKeyValuePair interpolationKeyValuePair || values[1] is not ItemsControl itemsControl)
        {
            return false;
        }

        {
            int index = itemsControl.Items.IndexOf(interpolationKeyValuePair);
            return index > 1;
        }
    }
}