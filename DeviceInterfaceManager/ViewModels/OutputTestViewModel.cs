using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceInterfaceManager.Services.Devices;
using Component = DeviceInterfaceManager.Models.Devices.Component;

namespace DeviceInterfaceManager.ViewModels;

public partial class OutputTestViewModel : ObservableObject
{
    public IDeviceService DeviceService { get; }

    public OutputTestViewModel(IDeviceService deviceService)
    {
        DeviceService = deviceService;
    }

    [RelayCommand]
    private async Task TurnLedOnOff(Component component)
    {
        await DeviceService.SetLedAsync(component.Position, component.IsSet);
    }

    [RelayCommand]
    private async Task TurnAllLedsOn()
    {
        await SetAllLeds(true);
    }

    [RelayCommand]
    private async Task TurnAllLedsOff()
    {
        await SetAllLeds(false);
    }

    private async Task SetAllLeds(bool direction)
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

    [RelayCommand]
    private async Task TurnDatalineOnOff(Component component)
    {
        await DeviceService.SetDatalineAsync(component.Position, component.IsSet);
    }

    [RelayCommand]
    private async Task TurnAllDatalinesOn()
    {
        await SetAllDatalines(true);
    }

    [RelayCommand]
    private async Task TurnAllDatalinesOff()
    {
        await SetAllDatalines(false);
    }

    private async Task SetAllDatalines(bool direction)
    {
        if (DeviceService.Outputs is null)
        {
            return;
        }

        foreach (Component datalineComponent in DeviceService.Outputs.Dataline.Components)
        {
            datalineComponent.IsSet = direction;
            await DeviceService.SetDatalineAsync(datalineComponent.Position, direction);
        }
    }

    [RelayCommand]
    private async Task SetAnalog(Component component)
    {
        await DeviceService.SetAnalogAsync(component.Position, component.Value);
    }

    [RelayCommand]
    private async Task SetAllAnalogsMax()
    {
        await SetAllAnalogs(255);
    }

    [RelayCommand]
    private async Task SetAllAnalogsMin()
    {
        await SetAllAnalogs(0);
    }

    private async Task SetAllAnalogs(int value)
    {
        if (DeviceService.Outputs is null)
        {
            return;
        }

        foreach (Component analogComponent in DeviceService.Outputs.Analog.Components)
        {
            analogComponent.Value = value;
            await DeviceService.SetAnalogAsync(analogComponent.Position, value);
        }
    }

    [RelayCommand]
    private async Task SetSevenSegment(Component component)
    {
        if (component.StringValue is null)
        {
            return;
        }

        await DeviceService.SetSevenSegmentAsync(component.Position, component.StringValue);
    }

    [RelayCommand]
    private async Task FillAllSevenSegments()
    {
        await SetAllSevenSegments("8");
    }
    
    [RelayCommand]
    private async Task ClearAllSevenSegments()
    {
        await SetAllSevenSegments(" ");
    }

    private async Task SetAllSevenSegments(string value)
    {
        if (DeviceService.Outputs is null)
        {
            return;
        }

        foreach (Component segmentComponent in DeviceService.Outputs.SevenSegment.Components)
        {
            await DeviceService.SetSevenSegmentAsync(segmentComponent.Position, value);
        }
    }
}