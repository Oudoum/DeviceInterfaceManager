using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DeviceInterfaceManager.Devices;

public readonly struct ComponentInfo(int count, int first, int last)
{
    public int Count { get; } = count;
    public int First { get; } = first;
    public int Last { get; } = last;
    public IEnumerable<Component> Components { get; } = Component.GetComponents(first, last);
    public IEnumerable<int> Range { get; } = Enumerable.Range(first, count);
    
    public async Task PerformOperationOnRange(Func<int, Task> operationOnElement)
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
        for (int i = first; i <= last; i++)
        {
            components.Add(new Component(i));
        }

        return components;
    }
}