using Avalonia.Controls;
using Avalonia.Input;
using DeviceInterfaceManager.Models.Devices;
using FluentAvalonia.UI.Windowing;

namespace DeviceInterfaceManager.Views;

public partial class MainWindow : AppWindow
{
    
    public MainWindow()
    {
        InitializeComponent();
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.KeyModifiers != KeyModifiers.Control || sender is not StackPanel { DataContext: not null } stackPanel)
        {
            return;
        }
        
        DataObject data = new();
        data.Set(nameof(IInputOutputDevice), stackPanel.DataContext);
        DragDrop.DoDragDrop(e, data, DragDropEffects.Link);
    }
}