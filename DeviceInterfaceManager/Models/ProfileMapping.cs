using CommunityToolkit.Mvvm.ComponentModel;

#pragma warning disable CS0657 // Not a valid attribute location for this declaration

namespace DeviceInterfaceManager.Models;

public partial class ProfileMapping : ObservableObject
{
    [ObservableProperty]
    private bool _isActive = true;

    [ObservableProperty]
    private string? _profileName;

    [ObservableProperty]
    private string? _id;

    [ObservableProperty]
    private string? _deviceName;
}