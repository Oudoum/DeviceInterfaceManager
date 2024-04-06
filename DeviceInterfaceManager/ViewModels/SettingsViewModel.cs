using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceInterfaceManager.Models;
using DeviceInterfaceManager.Models.Devices;
using DeviceInterfaceManager.Models.Devices.interfaceIT.USB;
using Microsoft.Extensions.Configuration;
using WritableJsonConfiguration;

namespace DeviceInterfaceManager.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ObservableCollection<IInputOutputDevice> _inputOutputDevices;

    private readonly IConfiguration _configuration;
    
    public SettingsViewModel(ObservableCollection<IInputOutputDevice> inputOutputDevices, IConfiguration configuration)
    {
        _inputOutputDevices = inputOutputDevices;
        _configuration = configuration;
        Settings = configuration.Get<Settings>() ?? new Settings(); 
    }
    
#if DEBUG
    public SettingsViewModel()
    {
        _inputOutputDevices = new ObservableCollection<IInputOutputDevice>();
        _configuration = WritableJsonConfigurationFabric.Create("settings.json");
        Settings = new Settings();
    }
#endif

    public Settings Settings { get; }

    private void UpdateAppSettings(string name)
    {
        PropertyInfo? propertyInfo = Settings.GetType().GetProperty(name);

        if (propertyInfo is null)
        {
            return;
        }

        object? propertyValue = propertyInfo.GetValue(Settings);
        _configuration.Set(name, propertyValue);
    }

    public async Task Startup()
    {
        if (Settings.FdsUsb)
        {
          await ToggleFdsUsbAsync();
        }
    }

    [ObservableProperty]
    private string? _wasmModuleUpdaterMessage;

    [RelayCommand]
    private static void OpenDiscord() => System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("https://discord.gg/8SS5ew4WvT") { UseShellExecute = true });

    [RelayCommand]
    private async Task UpdateDimWasmModuleAsync()
    {
        WasmModuleUpdaterMessage = await WasmModuleUpdater.Create().InstallWasmModule();
    }

    [RelayCommand]
    private void ToggleAutoHide() => UpdateAppSettings(nameof(Settings.AutoHide));
    
    [RelayCommand]
    private void ToggleMinimizedHide() => UpdateAppSettings(nameof(Settings.MinimizedHide));

    [RelayCommand]
    private async Task ToggleFdsUsbAsync()
    {
        UpdateAppSettings(nameof(Settings.FdsUsb));
        
        if (!Settings.FdsUsb)
        {
            foreach (IInputOutputDevice inputOutputDevice in _inputOutputDevices.ToArray())
            {
                if (inputOutputDevice is not InterfaceItData)
                {
                    continue;
                }
        
                inputOutputDevice.Disconnect();
                _inputOutputDevices.Remove(inputOutputDevice);
            }
            
            return;
        }
        
        for (int i = 0; i < InterfaceItData.TotalControllers ; i++)
        {
            InterfaceItData interfaceItData = new();
            await interfaceItData.ConnectAsync();
            _inputOutputDevices.Add(interfaceItData);
        }
    }
}