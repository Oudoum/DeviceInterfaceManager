using CommunityToolkit.Mvvm.ComponentModel;
using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.ViewModels;

public class InformationViewModel(IInputOutputDevice inputOutputDevice) : ObservableObject
{
    public IInputOutputDevice InputOutputDevice => inputOutputDevice;

    public string ButtonInformation => $"{InputOutputDevice.Switch.Count} | " +
                                       $"( {InputOutputDevice.Switch.First} - " +
                                       $"{InputOutputDevice.Switch.Last} )";

    public string LedInformation => $"{InputOutputDevice.Led.Count} | " +
                                    $"( {InputOutputDevice.Led.First} - " +
                                    $"{InputOutputDevice.Led.Last} )";

    public string DatalineInformation => $"{InputOutputDevice.Dataline.Count} | " +
                                         $"( {InputOutputDevice.Dataline.First} - " +
                                         $"{InputOutputDevice.Dataline.Last} )";

    public string SevenSegmentInformation => $"{InputOutputDevice.SevenSegment.Count} | " +
                                             $"( {InputOutputDevice.SevenSegment.First} - " +
                                             $"{InputOutputDevice.SevenSegment.Last} )";
}