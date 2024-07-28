using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceInterfaceManager.Models.Devices;
using Component = DeviceInterfaceManager.Models.Devices.Component;

namespace DeviceInterfaceManager.ViewModels;

public partial class OutputTestViewModel(IInputOutputDevice inputOutputDevice) : ObservableObject
{
    #if DEBUG
    public OutputTestViewModel() : this(new DeviceSerialBase())
    {
        
    }
    #endif
    
    public IInputOutputDevice InputOutputDevice => inputOutputDevice;
    
    [ObservableProperty]
    private Component? _selectedDataline = inputOutputDevice.Dataline.Components.FirstOrDefault();

    [RelayCommand]
    private async Task SetDataline(bool direction)
    {
        if (SelectedDataline is null)
        {
            return;
        }
        
        await InputOutputDevice.SetDatalineAsync(SelectedDataline.Position, direction);
    }
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SevenSegmentText))]
    private Component? _selectedSevenSegment = inputOutputDevice.SevenSegment.Components.FirstOrDefault();

    partial void OnSelectedSevenSegmentChanged(Component? value)
    {
        if (value is null)
        {
            return;
        }

        Reset7SegmentDisplay();
        SevenSegmentText = _sevenSegmentText;
    }

    private string _sevenSegmentText = string.Empty;

    public string SevenSegmentText
    {
        get => _sevenSegmentText;
        set
        {
            if (SelectedSevenSegment is null)
            {
                return;
            }
            
            if (SevenSegmentText.Length > value.Length)
            {
                Reset7SegmentDisplay();
            }

            int length = InputOutputDevice.SevenSegment.Count - SelectedSevenSegment.Position + InputOutputDevice.SevenSegment.First;
            if (value.Length > length)
            {
                _sevenSegmentText = value.Remove(length);
                InputOutputDevice.SetSevenSegmentAsync(SelectedSevenSegment.Position, _sevenSegmentText);
                return;
            }

            InputOutputDevice.SetSevenSegmentAsync(SelectedSevenSegment.Position, _sevenSegmentText = value);
        }
    }

    private void Reset7SegmentDisplay()
    {
        InputOutputDevice.SetSevenSegmentAsync(InputOutputDevice.SevenSegment.First, new string(' ', InputOutputDevice.SevenSegment.Count));
    }

    [ObservableProperty]
    private int _analogOutValue;

    partial void OnAnalogOutValueChanged(int value)
    {
        InputOutputDevice.SetAnalogAsync(InputOutputDevice.AnalogOut.First, value);
    }

    [RelayCommand]
    private async Task IsChecked(Component component)
    {
        await InputOutputDevice.SetLedAsync(component.Position, component.IsSet);
    }

    [RelayCommand]
    private async Task SetAllLed(bool direction)
    {
        foreach (Component ledComponent in InputOutputDevice.Led.Components)
        {
            ledComponent.IsSet = direction;
            await InputOutputDevice.SetLedAsync(ledComponent.Position, direction);
        }
    }
}