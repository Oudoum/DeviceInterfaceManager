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
        
        DataTransfer data = new();
        DataTransferItem item = new();
        item.Set(DataFormat.CreateStringApplicationFormat("DeviceId"), ((IDeviceService)stackPanel.DataContext).Id);
        item.Set(DataFormat.CreateStringApplicationFormat("DeviceName"), ((IDeviceService)stackPanel.DataContext).DeviceName);
        data.Add(item);
        await DragDrop.DoDragDropAsync(e, data, DragDropEffects.Link).ConfigureAwait(false);
    }
}