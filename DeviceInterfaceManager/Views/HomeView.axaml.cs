using Avalonia.Controls;
using Avalonia.Input;
using DeviceInterfaceManager.Models;
using DeviceInterfaceManager.Services.Devices;

namespace DeviceInterfaceManager.Views;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();

        AddHandler(DragDrop.DropEvent, OnDrop);
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
    }

    private async void ProfileListOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsRightButtonPressed || sender is not StackPanel { DataContext: not null } stackPanel)
        {
            return;
        }

        DataObject data = new();
        data.Set(nameof(ProfileCreatorModel), stackPanel.DataContext);
        await DragDrop.DoDragDrop(e, data, DragDropEffects.Link);
    }

    private static void OnDrop(object? sender, DragEventArgs e)
    {
        DropLogic(e, true);
    }

    private static void DropLogic(DragEventArgs e, bool set = false)
    {
        object? data = e.Data.Get(nameof(IDeviceService)) ?? e.Data.Get(nameof(ProfileCreatorModel));

        if (e.Source is not Control control)
        {
            return;
        }

        if (control.DataContext is not ProfileMapping profileMapping)
        {
            return;
        }

        switch (control.Name)
        {
            case "DeviceStackPanel" when data is IDeviceService inputOutputDevice && (string.IsNullOrEmpty(profileMapping.DeviceName) || profileMapping.DeviceName == inputOutputDevice.DeviceName):
                e.DragEffects = DragDropEffects.Link;
                if (set)
                {
                    profileMapping.Id = inputOutputDevice.Id;
                    profileMapping.DeviceName = inputOutputDevice.DeviceName;
                }
                break;

            case "ProfileStackPanel" when data is ProfileCreatorModel profileCreatorModel && (string.IsNullOrEmpty(profileMapping.DeviceName) || profileMapping.DeviceName == profileCreatorModel.DeviceName):
                e.DragEffects = DragDropEffects.Link;
                if (set)
                {
                    profileMapping.ProfileName = profileCreatorModel.ProfileName;
                    profileMapping.DeviceName = profileCreatorModel.DeviceName;
                }
                break;
        }
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = DragDropEffects.None;
        DropLogic(e);
    }

    private void DataGridOnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (e.Source is not Control { DataContext: ProfileMapping profileMapping } control)
        {
            return;
        }

        switch (control.Name)
        {
            case "DeviceStackPanel":
                profileMapping.Id = null;
                if (string.IsNullOrEmpty(profileMapping.ProfileName))
                {
                    profileMapping.DeviceName = null;
                }
                break;

            case "ProfileStackPanel":
                profileMapping.ProfileName = null;
                if (string.IsNullOrEmpty(profileMapping.Id))
                {
                    profileMapping.DeviceName = null;
                }
                break;
        }
    }
}