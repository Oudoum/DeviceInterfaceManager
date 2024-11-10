using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using DeviceInterfaceManager.Services.Devices;
using Microsoft.Extensions.Logging;

namespace DeviceInterfaceManager.ViewModels;

public class DeviceViewModel : ObservableObject
{
    public DeviceViewModel(IDeviceService deviceService)
    {
        DeviceService = deviceService;
        InformationViewModel = new InformationViewModel(deviceService);
        InputTestViewModel = new InputTestViewModel(deviceService);
        OutputTestViewModel = new OutputTestViewModel(Ioc.Default.GetService<ILogger<OutputTestViewModel>>()!, deviceService);
    }
    
    public IDeviceService DeviceService { get; }

    public InformationViewModel InformationViewModel { get; }

    public InputTestViewModel InputTestViewModel { get; }

    public OutputTestViewModel OutputTestViewModel { get; }

    public bool IsHomeSelected { get; set; } = true;

    public bool IsInputTestSelected { get; set; }

    public bool IsOutputTestSelected { get; set; }
}