using CommunityToolkit.Mvvm.ComponentModel;

namespace DeviceInterfaceManager.ViewModels;

public partial class InformationViewModel(DeviceItem deviceItem) : ObservableObject
{
    [ObservableProperty]
    private DeviceItem _deviceItem = deviceItem;

    [ObservableProperty]
    private string? _buttonInformation = $"{deviceItem.InputOutputDevice.Switch.Count} | ( {deviceItem.InputOutputDevice.Switch.First} - {deviceItem.InputOutputDevice.Switch.Last} )";

    [ObservableProperty]
    private string? _ledInformation = $"{deviceItem.InputOutputDevice.Led.Count} | ( {deviceItem.InputOutputDevice.Led.First} - {deviceItem.InputOutputDevice.Led.Last} )";

    [ObservableProperty]
    private string? _datalineInformation = $"{deviceItem.InputOutputDevice.Dataline.Count} | ( {deviceItem.InputOutputDevice.Dataline.First} - {deviceItem.InputOutputDevice.Dataline.Last} )";

    [ObservableProperty]
    private string? _sevenSegmentInformation = $"{deviceItem.InputOutputDevice.SevenSegment.Count} | ( {deviceItem.InputOutputDevice.SevenSegment.First} - {deviceItem.InputOutputDevice.SevenSegment.Last} )";
}