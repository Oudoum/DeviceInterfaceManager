using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

#pragma warning disable CS0657 // Not a valid attribute location for this declaration

namespace DeviceInterfaceManager.Models.Devices;

public class ComponentInfo
{
    public ComponentInfo(int first, int last)
    {
        First = first;
        Last = last;
        Components = Component.GetComponents(first, last);
    }

    public ComponentInfo(IEnumerable<Component> components)
    {
        var enumerable = components as Component[] ?? components.ToArray();
        Components = enumerable;
        First = enumerable.MinBy(x => x.Position)?.Position ?? default;
        Last = enumerable.MaxBy(x => x.Position)?.Position ?? default;
    }

    public int Count => Components.Count();

    public int First { get; }

    public int Last { get; }

    public IEnumerable<Component> Components { get; }

    public void UpdatePosition(int position, bool isSet)
    {
        Component? component = Components.FirstOrDefault(c => c.Position == position);
        if (component is not null)
        {
            component.IsSet = isSet;
        }
    }

    public void UpdatePosition(int position, int value)
    {
        Component? component = Components.FirstOrDefault(c => c.Position == position);
        if (component is not null)
        {
            component.Value = value;
        }
    }

    public async Task PerformOperationOnAllComponents(Func<int, Task> operationOnElement)
    {
        for (int i = First; i <= Last; i++)
        {
            await operationOnElement(i);
        }
    }
}

public partial class Component : ObservableObject
{
    public Component(int position)
    {
        Position = position;
        Name = position.ToString();
    }

    public Component(int position, string name)
    {
        Position = position;
        Name = name;
    }

    public int Position { get; }

    [JsonIgnore] public string? Name { get; }

    [ObservableProperty]
    [property: JsonIgnore]
    private bool _isSet;

    [ObservableProperty]
    [property: JsonIgnore]
    private int _value;

    public static IEnumerable<Component> GetComponents(int first, int last)
    {
        List<Component> components = [];
        if (first == 0 && last == 0)
        {
            return components;
        }

        for (int i = first; i <= last; i++)
        {
            components.Add(new Component(i));
        }

        return components;
    }
}