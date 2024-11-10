using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceInterfaceManager.Services.Devices;
using Microsoft.Extensions.Logging;
using Component = DeviceInterfaceManager.Models.Devices.Component;

namespace DeviceInterfaceManager.ViewModels;

public partial class OutputTestViewModel : ObservableObject
{
    private readonly ILogger _logger;
    
    public OutputTestViewModel(ILogger<OutputTestViewModel> logger, IDeviceService deviceService)
    {
        _logger = logger;
        DeviceService = deviceService;
        SelectedDataline = DeviceService.Outputs?.Dataline.Components.FirstOrDefault();
        SelectedSevenSegment = DeviceService.Outputs?.SevenSegment.Components.FirstOrDefault();
        SelectedAnalog = DeviceService.Outputs?.Analog.Components.FirstOrDefault();
    }
    
#if DEBUG
    public OutputTestViewModel()
    {
        _logger = new LoggerFactory().CreateLogger<OutputTestViewModel>();
        DeviceService = new DeviceSerialService();
    }
#endif

    public IDeviceService DeviceService { get; }

    [ObservableProperty]
    private Component? _selectedDataline;

    [RelayCommand]
    private async Task SetDataline(bool direction)
    {
        if (SelectedDataline is null)
        {
            return;
        }

        await DeviceService.SetDatalineAsync(SelectedDataline.Position, direction);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SevenSegmentText))]
    private Component? _selectedSevenSegment;

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
            if (DeviceService.Outputs is null || SelectedSevenSegment is null)
            {
                return;
            }

            if (SevenSegmentText.Length > value.Length)
            {
                Reset7SegmentDisplay();
            }

            int length = DeviceService.Outputs.SevenSegment.Count - SelectedSevenSegment.Position + DeviceService.Outputs.SevenSegment.First;
            if (value.Length > length)
            {
                _sevenSegmentText = value.Remove(length);
                DeviceService.SetSevenSegmentAsync(SelectedSevenSegment.Position, _sevenSegmentText);
                return;
            }

            DeviceService.SetSevenSegmentAsync(SelectedSevenSegment.Position, _sevenSegmentText = value);
        }
    }

    private void Reset7SegmentDisplay()
    {
        if (DeviceService.Outputs is null)
        {
            return;
        }

        DeviceService.SetSevenSegmentAsync(DeviceService.Outputs.SevenSegment.First, new string(' ', DeviceService.Outputs.SevenSegment.Count));
    }

    [ObservableProperty]
    private Component? _selectedAnalog;

    [ObservableProperty]
    private int _analogValue;

    partial void OnAnalogValueChanged(int value)
    {
        if (DeviceService.Outputs is null || SelectedAnalog is null)
        {
            return;
        }
        
        DeviceService.SetAnalogAsync(SelectedAnalog.Position, value);
    }

    [RelayCommand]
    private async Task IsChecked(Component component)
    {
        await DeviceService.SetLedAsync(component.Position, component.IsSet);
    }

    [RelayCommand]
    private async Task SetAllLed(bool direction)
    {
        if (DeviceService.Outputs is null)
        {
            return;
        }
        
        foreach (Component ledComponent in DeviceService.Outputs.Led.Components)
        {
            ledComponent.IsSet = direction;
            await DeviceService.SetLedAsync(ledComponent.Position, direction);
        }
    }
}