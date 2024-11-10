using CommunityToolkit.Mvvm.ComponentModel;
using DeviceInterfaceManager.Services.Devices;

namespace DeviceInterfaceManager.ViewModels;

public class InformationViewModel : ObservableObject
{
    public InformationViewModel(IDeviceService deviceService)
    {
        DeviceService = deviceService;
    }
    
    public IDeviceService DeviceService { get; }
}