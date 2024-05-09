using CommunityToolkit.Mvvm.ComponentModel;
using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.ViewModels;

public class InformationViewModel(IInputOutputDevice inputOutputDevice) : ObservableObject
{
    public IInputOutputDevice InputOutputDevice => inputOutputDevice;
}