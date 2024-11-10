using Avalonia.Controls;
using Avalonia.Input;
using DeviceInterfaceManager.Services.Devices;
using FluentAvalonia.UI.Windowing;

namespace DeviceInterfaceManager.Views;

public partial class MainWindow : AppWindow
{
    
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void DeviceOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsRightButtonPressed || sender is not StackPanel { DataContext: not null } stackPanel)
        {
            return;
        }
        
        DataObject data = new();
        data.Set(nameof(IDeviceService), stackPanel.DataContext);
        await DragDrop.DoDragDrop(e, data, DragDropEffects.Link);
    }
}