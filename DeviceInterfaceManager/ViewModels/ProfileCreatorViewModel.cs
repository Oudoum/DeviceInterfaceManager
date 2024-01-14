using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DeviceInterfaceManager.ViewModels;

public partial class ProfileCreatorViewModel(ObservableCollection<DeviceItem> deviceItems) : ObservableObject
{
    private ObservableCollection<DeviceItem> _deviceItems = deviceItems;
}