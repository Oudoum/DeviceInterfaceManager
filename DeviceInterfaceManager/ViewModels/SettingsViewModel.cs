using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceInterfaceManager.Models;
using DeviceInterfaceManager.Models.Devices;
using DeviceInterfaceManager.Models.Devices.interfaceIT.ENET;
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
    
    public FlightSimulatorDataServer? FlightSimulatorDataServer { get; private set; }

    public async Task Startup()
    {
        if (Settings.FdsUsb)
        {
            ToggleFdsUsbCommand.Execute(null);
        }

        if (Settings.FdsEthernet)
        {
            ToggleFdsEthernetCommand.Execute(null);
        }

        if (Settings.Server)
        {
            StartServerCommand.Execute(null);
        }
        
        UpdateManager updateManager = CreateUpdateManager();

        CurrentVersion = updateManager.CurrentVersion?.ToFullString();

        if (Settings.CheckForUpdates)
        {
            await CheckForUpdatesAsync(updateManager);
        }
    }

    [ObservableProperty]
    private string? _currentVersion;

    private static UpdateManager CreateUpdateManager()
    {
        return new UpdateManager(new GithubSource("https://github.com/Oudoum/DeviceInterfaceManager", null, false));
    }

    [ObservableProperty]
    private bool _isUpToDate;

    [RelayCommand]
    private async Task CheckForUpdatesAsync(UpdateManager? updateManager)
    {
        IsUpToDate = false;

        updateManager ??= CreateUpdateManager();

        if (updateManager.IsInstalled)
        {
            UpdateInfo? newVersion = await updateManager.CheckForUpdatesAsync();

            if (newVersion is null)
            {
                IsUpToDate = true;
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
    private static void OpenUserDataFolder()
    {
        Process.Start("explorer.exe", App.UserDataPath);
    }

    private void DisconnectAndRemove<T>() where T : IInputOutputDevice
    {
        foreach (IInputOutputDevice inputOutputDevice in inputOutputDevices.ToArray())
        {
            if (inputOutputDevice is not T)
            {
                continue;
            }

            inputOutputDevice.Disconnect();
            inputOutputDevices.Remove(inputOutputDevice);
        }
    }

    [RelayCommand]
    private async Task StartServer()
    {
        if (FlightSimulatorDataServer is not null)
        {
            return;    
        }
        
        FlightSimulatorDataServer = new FlightSimulatorDataServer();
        await FlightSimulatorDataServer.StartAsync(Settings.IpAddress, Settings.Port);
    }

    #region FdsUsb
    
    [RelayCommand]
    private async Task ToggleFdsUsbAsync(CancellationToken cancellationToken)
    {
        if (!Settings.FdsUsb)
        {
            DisconnectAndRemove<InterfaceItData>();
            return;
        }

        for (int i = 0; i < InterfaceItData.TotalControllers; i++)
        {
            InterfaceItData interfaceItData = new();
            if (await interfaceItData.ConnectAsync(cancellationToken) == ConnectionStatus.Connected)
            {
                inputOutputDevices.Add(interfaceItData);
            }
        }
    }
    
    #endregion

    #region FdsEthernet
    
    [RelayCommand]
    private async Task ToggleFdsEthernetAsync(CancellationToken cancellationToken)
    {
        if (!Settings.FdsEthernet)
        {
            DisconnectAndRemove<InterfaceItEthernet>();
            return;
        }

        if (Settings.FdsEthernetConnections is not null)
        {
            foreach (string fdsEthernetConnection in Settings.FdsEthernetConnections)
            {
                if (inputOutputDevices.Any(x => x.Id == fdsEthernetConnection))
                {
                    continue;
                }

                InterfaceItEthernet interfaceItEthernet = new(fdsEthernetConnection);
                if (await interfaceItEthernet.ConnectAsync(cancellationToken) == ConnectionStatus.Connected)
                {
                    inputOutputDevices.Add(interfaceItEthernet);
                }
            }
        }
    }

    [RelayCommand]
    private void AddInterfaceItEthernetConnection(string connection)
    {
        if (!IPAddress.TryParse(connection, out IPAddress? ipAddress))
        {
            return;
        }

        connection = ipAddress.ToString();
        if (Settings.FdsEthernetConnections is null || Settings.FdsEthernetConnections.Contains(connection))
        {
            return;
        }

        Settings.FdsEthernetConnections?.Add(connection);
        ToggleFdsEthernetCommand.Execute(null);
    }

    [RelayCommand]
    private void RemoveInterfaceItEthernetConnection(string connection)
    {
        Settings.FdsEthernetConnections?.Remove(connection);
    }

    [RelayCommand]
    private async Task GetInterfaceItEthernetDevices()
    {
        string connection = await InterfaceItEthernet.ReceiveControllerDiscoveryDataAsync();
        if (!string.IsNullOrEmpty(connection))
        {
            AddInterfaceItEthernetConnection(connection);
        }
    }
    
    #endregion
}