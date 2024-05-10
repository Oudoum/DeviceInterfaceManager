using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceInterfaceManager.Models;
using DeviceInterfaceManager.Models.Devices;
using DeviceInterfaceManager.Models.Devices.interfaceIT.USB;
using Velopack;
using Velopack.Sources;

namespace DeviceInterfaceManager.ViewModels;

public partial class SettingsViewModel(ObservableCollection<IInputOutputDevice> inputOutputDevices) : ObservableObject
{
    
#if DEBUG
    public SettingsViewModel() : this([])
    {
        Settings = new Settings();
    }
#endif

    public Settings Settings { get; } = Settings.CreateSettings();

    public async Task Startup()
    {
        UpdateManager updateManager = CreateUpdateManager();
        
        CurrentVersion = updateManager.CurrentVersion?.ToFullString();
        
        if (Settings.CheckForUpdates)
        {
           await CheckForUpdatesAsync(updateManager);
        }
        
        if (Settings.FdsUsb)
        {
          await ToggleFdsUsbAsync();
        }
    }

    [ObservableProperty]
    private string? _currentVersion;

    private static UpdateManager CreateUpdateManager()
    {
        return new UpdateManager(new GithubSource("https://github.com/Oudoum/DeviceInterfaceManager", null, false));
    }

    [RelayCommand]
    private static async Task CheckForUpdatesAsync(UpdateManager? updateManager)
    {
        updateManager ??= CreateUpdateManager();

        if (updateManager.IsInstalled)
        {
            UpdateInfo? newVersion = await updateManager.CheckForUpdatesAsync();
            
            if (newVersion is null)
            {
                return;
            }
            
            await updateManager.DownloadUpdatesAsync(newVersion);
        
            updateManager.ApplyUpdatesAndRestart(newVersion);
        }
    }

    [ObservableProperty]
    private string? _wasmModuleUpdaterMessage;

    [RelayCommand]
    private async Task UpdateDimWasmModuleAsync()
    {
        WasmModuleUpdaterMessage = await WasmModuleUpdater.Create().InstallWasmModule();
    }

    [RelayCommand]
    private async Task ToggleFdsUsbAsync()
    {
        if (!Settings.FdsUsb)
        {
            foreach (IInputOutputDevice inputOutputDevice in inputOutputDevices.ToArray())
            {
                if (inputOutputDevice is not InterfaceItData)
                {
                    continue;
                }
        
                inputOutputDevice.Disconnect();
                inputOutputDevices.Remove(inputOutputDevice);
            }
            
            return;
        }
        
        for (int i = 0; i < InterfaceItData.TotalControllers ; i++)
        {
            InterfaceItData interfaceItData = new();
            await interfaceItData.ConnectAsync();
            inputOutputDevices.Add(interfaceItData);
        }
    }
}