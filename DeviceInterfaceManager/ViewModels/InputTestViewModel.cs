using CommunityToolkit.Mvvm.ComponentModel;
using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.ViewModels;

public class InputTestViewModel(IInputOutputDevice inputOutputDevice) : ObservableObject
{
#if DEBUG
    public InputTestViewModel() : this(new DeviceSerialBase())
    {
    }
#endif

    public IInputOutputDevice InputOutputDevice => inputOutputDevice;
}