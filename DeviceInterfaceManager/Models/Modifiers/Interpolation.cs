using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

#pragma warning disable CS0657 // Not a valid attribute location for this declaration

namespace DeviceInterfaceManager.Models.Modifiers;

public partial class Interpolation : ObservableObject, IModifier
{
    public Interpolation()
    {
        Values = [];
        Add(false);
        Add(false);
    }

    private void ItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(InterpolationKeyValuePair.Value) || sender is not InterpolationKeyValuePair)
        {
            return;
        }

        UpdateMinMaxValues();
    }

    private void UpdateMinMaxValues()
    {
        if (Values.Count <= 0)
        {
            return;
        }

        Min = Values.Min(x => x.Value);
        Max = Values.Max(x => x.Value);
    }

    [ObservableProperty]
    private bool _isActive = true;

    [ObservableProperty]
    private ObservableCollection<InterpolationKeyValuePair> _values = [];

    partial void OnValuesChanged(ObservableCollection<InterpolationKeyValuePair> value)
    {
        value.CollectionChanged += (_, args) =>
        {
            if (args.NewItems is not null)
            {
                foreach (InterpolationKeyValuePair item in args.NewItems)
                {
                    item.PropertyChanged += ItemPropertyChanged;
                }
            }

            if (args.OldItems is not null)
            {
                foreach (InterpolationKeyValuePair item in args.OldItems)
                {
                    item.PropertyChanged -= ItemPropertyChanged;
                }
            }
            
            UpdateMinMaxValues();
        };

        foreach (InterpolationKeyValuePair valuePair in value)
        {
            valuePair.PropertyChanged += ItemPropertyChanged;
        }

        UpdateMinMaxValues();
    }

    [ObservableProperty]
    [property: JsonIgnore]
    private double _min;

    [ObservableProperty]
    [property: JsonIgnore]
    private double _max;

    [RelayCommand]
    private void AddItem()
    {
        Add(true);
    }

    private void Add(bool isVisible)
    {
        int count = Values.Count;
        double key = 0;
        double value = 0;
        if (Values.Count > 0)
        {
            key = Values.First().Key;
            value = Values.First().Value;
        }

        Values.Add(new InterpolationKeyValuePair(100 * count - key * (count - 1), 1024 * count - value * (count - 1), isVisible));
    }

    [RelayCommand]
    private void RemoveItem(InterpolationKeyValuePair item)
    {
        Values.Remove(item);
    }

    public void Apply(ref StringBuilder value)
    {
        string sValue = value.ToString();
        if (!double.TryParse(sValue, CultureInfo.InvariantCulture, out double simValue))
        {
            return;
        }

        double first = Values.First().Key;
        if (simValue <= first)
        {
            value = new StringBuilder(Values.First().Value.ToString(CultureInfo.InvariantCulture));
            return;
        }

        double second = Values.Last().Key;
        if (simValue >= second)
        {
            value = new StringBuilder(Values.Last().Value.ToString(CultureInfo.InvariantCulture));
            return;
        }

        if (Values.Count > 2)
        {
            for (int i = 1; i != Values.Count; ++i)
            {
                double currentKey = Values.ElementAt(i).Key;
                if (currentKey <= simValue && currentKey > first)
                {
                    if (Math.Abs(currentKey - simValue) < 0)
                    {
                        value = new StringBuilder(Values.ElementAt(i).Value.ToString(CultureInfo.InvariantCulture));
                        return;
                    }

                    first = currentKey;
                    continue;
                }

                if (!(currentKey >= simValue) || !(currentKey < second))
                {
                    continue;
                }

                second = currentKey;
                if (Math.Abs(currentKey - simValue) < 0)
                {
                    value = new StringBuilder(Values.ElementAt(i).Value.ToString(CultureInfo.InvariantCulture));
                    return;
                }

                break;
            }
        }

        value = Interpolate(simValue, first, GetValueByKey(first), second, GetValueByKey(second));
    }

    private static StringBuilder Interpolate(double value, double x1, double y1, double x2, double y2)
    {
        return Math.Abs(x1 - x2) < 0 ? new StringBuilder(y1.ToString(CultureInfo.InvariantCulture)) : new StringBuilder((y1 + (y2 - y1) / (x2 - x1) * (value - x1)).ToString(CultureInfo.InvariantCulture));
    }

    public object Clone()
    {
        Interpolation clone = new();
        ObservableCollection<InterpolationKeyValuePair> valuePairs = [];
        foreach (InterpolationKeyValuePair keyValuePari in Values)
        {
            InterpolationKeyValuePair value = (InterpolationKeyValuePair)keyValuePari.Clone();
            valuePairs.Add(value);
        }
        clone.Values = valuePairs;

        return clone;
    }

    private double GetValueByKey(double key)
    {
        InterpolationKeyValuePair pair = Values.First(p => Math.Abs(p.Key - key) < 0.0001);
        return pair.Value;
    }

    public partial class InterpolationKeyValuePair : ObservableObject, ICloneable
    {
        public InterpolationKeyValuePair(double key, double value, bool isVisible)
        {
            Key = key;
            Value = value;
            IsVisible = isVisible;
        }

        public bool IsVisible { get; }
        public double Key { get; set; }

        [ObservableProperty]
        private double _value;

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}