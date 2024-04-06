using CommunityToolkit.Mvvm.ComponentModel;
using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.ViewModels;

public class InputTestViewModel(IInputOutputDevice inputOutputDevice) : ObservableObject
{
    public IInputOutputDevice InputOutputDevice => inputOutputDevice;
}