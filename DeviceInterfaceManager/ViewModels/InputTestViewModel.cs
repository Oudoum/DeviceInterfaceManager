using CommunityToolkit.Mvvm.ComponentModel;
using DeviceInterfaceManager.Models.Devices;
using DeviceInterfaceManager.Services.Devices;

namespace DeviceInterfaceManager.ViewModels;

public class InputTestViewModel : ObservableObject
{
    public InputTestViewModel(IDeviceService deviceService)
    {
        DeviceService = deviceService;
    }
    
#if DEBUG
    public InputTestViewModel()
    {
        DeviceService = new DeviceSerialService();
    }
#endif

    public IDeviceService DeviceService { get; }
}