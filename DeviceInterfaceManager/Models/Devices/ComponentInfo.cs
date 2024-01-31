using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DeviceInterfaceManager.Models.Devices;

public readonly struct ComponentInfo(int first, int last)
{
    public int Count => Components.Count();

    public int First { get; } = first;

    public int Last { get; } = last;
    
    public IEnumerable<Component> Components { get; } = Component.GetComponents(first, last);
    
    public void UpdatePosition(int position, bool isSet)
    {
        Component? component = Components.FirstOrDefault(c => c.Position == position);
        if (component is not null)
        {
            component.IsSet = isSet;
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

public partial class Component(int position) : ObservableObject
{
    public int Position { get; } = position;

    [ObservableProperty]
    private bool _isSet;

    public static IEnumerable<Component> GetComponents(int first, int last)
    {
        List<Component> components = [];
        if (first == 0 || last == 0)
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