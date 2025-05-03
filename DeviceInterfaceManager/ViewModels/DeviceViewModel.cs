using CommunityToolkit.Mvvm.ComponentModel;
using DeviceInterfaceManager.Services.Devices;

namespace DeviceInterfaceManager.ViewModels;

public class DeviceViewModel : ObservableObject
{
    public DeviceViewModel(IDeviceService deviceService)
    {
        DeviceService = deviceService;
        InformationViewModel = new InformationViewModel(deviceService);
        InputTestViewModel = new InputTestViewModel(deviceService);
        OutputTestViewModel = new OutputTestViewModel(deviceService);
    }

    public IDeviceService DeviceService { get; }

    public InformationViewModel InformationViewModel { get; }

    public InputTestViewModel InputTestViewModel { get; }

    public OutputTestViewModel OutputTestViewModel { get; }

    public bool IsHomeSelected { get; set; } = true;

    public bool IsInputTestSelected { get; set; }

    public bool IsOutputTestSelected { get; set; }
}