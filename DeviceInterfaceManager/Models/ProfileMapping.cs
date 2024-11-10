using CommunityToolkit.Mvvm.ComponentModel;

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

    [ObservableProperty]
    private string? _aircraft;
}