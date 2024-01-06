using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceInterfaceManager.Devices;

namespace DeviceInterfaceManager.ViewModels;

public partial class OutputTestViewModel(DeviceItem deviceItem) : ObservableObject
{
    [ObservableProperty]
    private IInputOutputDevice _inputOutputDevice = deviceItem.InputOutputDevice;

    [ObservableProperty]
    private int _datalineSelectedPosition;

    [RelayCommand]
    private async Task SetDataline(bool direction)
    {
        await InputOutputDevice.SetDatalineAsync(DatalineSelectedPosition.ToString(), direction);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SevenSegmentText))]
    private int _sevenSegmentSelectedPosition;

    partial void OnSevenSegmentSelectedPositionChanged(int value)
    {
        Reset7SegmentDisplay();
        SevenSegmentText = _sevenSegmentText;
    }

    private string _sevenSegmentText = string.Empty;

    public string SevenSegmentText
    {
        get => _sevenSegmentText;
        set
        {
            if (SevenSegmentText.Length > value.Length)
            {
                Reset7SegmentDisplay();
            }

            int length = InputOutputDevice.SevenSegment.Count - SevenSegmentSelectedPosition + InputOutputDevice.SevenSegment.First;
            if (value.Length > length)
            {
                _sevenSegmentText = value.Remove(length);
                InputOutputDevice.SetSevenSegmentAsync(SevenSegmentSelectedPosition.ToString(), _sevenSegmentText);
                return;
            }

            InputOutputDevice.SetSevenSegmentAsync(SevenSegmentSelectedPosition.ToString(), _sevenSegmentText = value);
        }
    }

    private void Reset7SegmentDisplay()
    {
        InputOutputDevice.SetSevenSegmentAsync(InputOutputDevice.SevenSegment.First.ToString(), new string(' ', InputOutputDevice.SevenSegment.Count));
    }

    [RelayCommand]
    private async Task IsChecked(Component component)
    {
        await InputOutputDevice.SetLedAsync(component.Position.ToString(), component.IsSet);
    }

    [RelayCommand]
    private async Task SetAllLed(bool direction)
    {
        foreach (Component ledComponent in InputOutputDevice.Led.Components)
        {
            ledComponent.IsSet = direction;
            await InputOutputDevice.SetLedAsync(ledComponent.Position.ToString(), direction);
        }
    }
}