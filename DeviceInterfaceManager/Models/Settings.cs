using CommunityToolkit.Mvvm.ComponentModel;

namespace DeviceInterfaceManager.Models;

public sealed partial class Settings : ObservableObject
{
    [ObservableProperty]
    private bool _minimizedHide;

    [ObservableProperty]
    private bool _autoHide;

    [ObservableProperty]
    private bool _isP3D;

   [ObservableProperty]
   private bool _fdsUsb;
}