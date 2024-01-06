using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace DeviceInterfaceManager;

public class ViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        if (data is null)
        {
            return new TextBlock { Text = "data was null" };
        }

        string? name = data.GetType().FullName!.Replace("ViewModel", "View");
        Type? type = Type.GetType(name);

        if (type is not null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }
        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        return data is ObservableObject;
    }
}