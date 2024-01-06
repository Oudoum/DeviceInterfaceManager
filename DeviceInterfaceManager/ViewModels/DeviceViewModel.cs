using CommunityToolkit.Mvvm.ComponentModel;
using DeviceInterfaceManager.Devices;

namespace DeviceInterfaceManager.ViewModels;

public partial class DeviceViewModel(DeviceItem deviceItem) : ObservableObject
{
    [ObservableProperty]
    private InformationViewModel _informationViewModel = new(deviceItem);

    [ObservableProperty]
    private InputTestViewModel _inputTestViewModel = new(deviceItem);

    [ObservableProperty]
    private OutputTestViewModel _outputTestViewModel = new(deviceItem);

    [ObservableProperty]
    private bool _isHomeSelected = true;

    [ObservableProperty]
    private bool _isInputTestSelected;

    [ObservableProperty]
    private bool _isOutputTestSelected;
}