using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

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
        First = enumerable.MinBy(x => x.Position)?.Position ?? 0;
        Last = enumerable.MaxBy(x => x.Position)?.Position ?? 0;
    }

    public int Count => Components.Count();

    public int First { get; }

    public int Last { get; }

    public IEnumerable<Component> Components { get; }

    public void UpdatePosition(int position, bool isSet)
    {
        Component? component = Components.FirstOrDefault(c => c.Position == position);
        component?.IsSet = isSet;
    }

    public void UpdatePosition(int position, int value)
    {
        Component? component = Components.FirstOrDefault(c => c.Position == position);
        component?.Value = value;
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
    private Component(int position)
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
    [JsonIgnore]
    public partial bool IsSet { get; set; }

    [ObservableProperty]
    [JsonIgnore]
    public partial int Value { get; set; }

    [ObservableProperty]
    [JsonIgnore]
    public partial string? StringValue { get; set; }

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