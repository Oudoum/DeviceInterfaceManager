using System;
using Avalonia.Controls;
using Avalonia.Input;
using DeviceInterfaceManager.Models;

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
        if (!e.GetCurrentPoint(this).Properties.IsRightButtonPressed || sender is not StackPanel { DataContext: Tuple<string, ProfileCreatorModel> tuple })
        {
            return;
        }

        DataTransfer data = new();
        DataTransferItem item = new();
        item.Set(DataFormat.CreateStringApplicationFormat("ProfileName"), tuple.Item1);
        item.Set(DataFormat.CreateStringApplicationFormat("DeviceName"), tuple.Item2.DeviceName);
        data.Add(item);
        await DragDrop.DoDragDropAsync(e, data, DragDropEffects.Link).ConfigureAwait(false);
    }

    private static void OnDrop(object? sender, DragEventArgs e)
    {
        DropLogic(e, true);
    }

    private static void DropLogic(DragEventArgs e, bool set = false)
    {
        if (e.Source is not Control control)
        {
            return;
        }

        if (control.DataContext is not ProfileMapping profileMapping)
        {
            return;
        }

        string? profileName = e.DataTransfer.TryGetValue(DataFormat.CreateStringApplicationFormat("ProfileName"));
        string? deviceId = e.DataTransfer.TryGetValue(DataFormat.CreateStringApplicationFormat("DeviceId"));
        string? deviceName = e.DataTransfer.TryGetValue(DataFormat.CreateStringApplicationFormat("DeviceName"));

        switch (control.Name)
        {
            case "DeviceStackPanel" when profileName is null:
                if (!string.IsNullOrEmpty(profileMapping.DeviceName) && profileMapping.DeviceName != deviceName)
                {
                    break;
                }

                e.DragEffects = DragDropEffects.Link;
                if (set)
                {
                    profileMapping.Id = deviceId;
                    profileMapping.DeviceName = deviceName;
                }

                break;

            case "ProfileStackPanel" when profileName is not null:
                if (!string.IsNullOrEmpty(profileMapping.DeviceName) && profileMapping.DeviceName != deviceName)
                {
                    break;
                }

                e.DragEffects = DragDropEffects.Link;
                if (set)
                {
                    profileMapping.ProfileName = profileName;
                    profileMapping.DeviceName = deviceName;
                }

                break;
        }
    }

    private static void OnDragOver(object? sender, DragEventArgs e)
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