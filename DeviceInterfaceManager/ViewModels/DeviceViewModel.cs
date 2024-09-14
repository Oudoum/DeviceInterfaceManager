using CommunityToolkit.Mvvm.ComponentModel;
using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.ViewModels;

public class DeviceViewModel(IInputOutputDevice inputOutputDevice) : ObservableObject
{
    public IInputOutputDevice InputOutputDevice => inputOutputDevice;

    public InformationViewModel InformationViewModel { get; } = new(inputOutputDevice);

    public InputTestViewModel InputTestViewModel { get; } = new(inputOutputDevice);

    public OutputTestViewModel OutputTestViewModel { get; } = new(inputOutputDevice);

    public bool IsHomeSelected { get; set; } = true;

    public bool IsInputTestSelected { get; set; }

    public bool IsOutputTestSelected { get; set; }
}