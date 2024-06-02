using CommunityToolkit.Mvvm.ComponentModel;

namespace DeviceInterfaceManager.Models;

public partial class ProfileMapping : ObservableObject
{
    public bool IsActive { get; set; } = true;

    [ObservableProperty]
    private string? _profileName;
    
    [ObservableProperty]
    private string? _id;

    [ObservableProperty]
    private string? _deviceName;
}