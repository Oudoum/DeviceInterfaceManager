using CommunityToolkit.Mvvm.ComponentModel;
using DeviceInterfaceManager.Services.Devices;

namespace DeviceInterfaceManager.ViewModels;

public class InputTestViewModel : ObservableObject
{
    public InputTestViewModel(IDeviceService deviceService)
    {
        DeviceService = deviceService;
    }
 
    public IDeviceService DeviceService { get; }
}