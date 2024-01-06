using CommunityToolkit.Mvvm.ComponentModel;
using DeviceInterfaceManager.Devices;

namespace DeviceInterfaceManager.ViewModels;

public partial class InputTestViewModel(DeviceItem deviceItem) : ObservableObject
{
    private DeviceItem _deviceItem = deviceItem;
}