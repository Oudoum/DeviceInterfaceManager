using CommunityToolkit.Mvvm.ComponentModel;
using DeviceInterfaceManager.Devices;

namespace DeviceInterfaceManager.ViewModels;

public partial class InputTestViewModel : ObservableObject
{
    public InputTestViewModel(DeviceItem deviceItem)
    {
        InputOutputDevice = deviceItem.InputOutputDevice;
    }

    [ObservableProperty]
    private IInputOutputDevice _inputOutputDevice;
}