using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using DeviceInterfaceManager.Models;
using DeviceInterfaceManager.Server;
using DeviceInterfaceManager.Services;
using DeviceInterfaceManager.Services.Devices;
using Microsoft.Extensions.Logging;
using Velopack;
using Velopack.Sources;
using InterfaceItUsbService = DeviceInterfaceManager.Services.Devices.InterfaceItUsbService;

namespace DeviceInterfaceManager.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ILogger _logger;
    private readonly ObservableCollection<IDeviceService> _inputOutputDevices;
    private readonly SignalRServerService _signalRServerService;
    private readonly SignalRClientService _signalRClientService;

    public SettingsViewModel(ILogger<SettingsViewModel> logger, ObservableCollection<IDeviceService> inputOutputDevices, SignalRServerService signalRServerService, SignalRClientService signalRClientService)
    {
        _logger = logger;
        _inputOutputDevices = inputOutputDevices;
        _signalRServerService = signalRServerService;
        _signalRClientService = signalRClientService;
    }
    
#if DEBUG
    public SettingsViewModel()
    {
        Settings = new Settings();
        _logger = new LoggerFactory().CreateLogger<SettingsViewModel>();
        _inputOutputDevices = [];
        _signalRServerService = Ioc.Default.GetRequiredService<SignalRServerService>();
        _signalRClientService = Ioc.Default.GetRequiredService<SignalRClientService>();
    }
#endif

    public Settings Settings { get; } = Settings.CreateSettings();

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

        if (Settings.FsCockpit)
        {
            ToggleFsCockpitCommand.Execute(null);
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
        WasmModuleUpdaterMessage = await WasmModuleUpdateService.Create().InstallWasmModule();
    }

    [RelayCommand]
    private static void OpenUserDataFolder()
    {
        Process.Start("explorer.exe", App.UserDataPath);
    }

    private void DisconnectAndRemove<T>() where T : IDeviceService
    {
        foreach (IDeviceService inputOutputDevice in _inputOutputDevices.ToArray())
        {
            if (inputOutputDevice is not T)
            {
                continue;
            }

            inputOutputDevice.Disconnect();
            _inputOutputDevices.Remove(inputOutputDevice);
        }
    }


    private bool _isStarted;

    [RelayCommand]
    private async Task StartServer(CancellationToken cancellationToken)
    {
        if (_isStarted)
        {
            await _signalRClientService.StopConnectionAsync(cancellationToken);
            await _signalRServerService.StopAsync(cancellationToken);
            _isStarted = false;
            return;
        }

        await _signalRServerService.StartAsync(Settings.IpAddress, Settings.Port, cancellationToken);
        await _signalRClientService.StartConnectionAsync(Settings.IpAddress, Settings.Port, cancellationToken);
        _isStarted = true;
    }

    #region FdsUsb

    [RelayCommand]
    private async Task ToggleFdsUsbAsync(CancellationToken cancellationToken)
    {
        if (!Settings.FdsUsb)
        {
            DisconnectAndRemove<InterfaceItUsbService>();
            return;
        }

        for (int i = 0; i < InterfaceItUsbService.TotalControllers; i++)
        {
            InterfaceItUsbService interfaceItUsbService = new();
            if (await interfaceItUsbService.ConnectAsync(cancellationToken) == ConnectionStatus.Connected)
            {
                _inputOutputDevices.Add(interfaceItUsbService);
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
            DisconnectAndRemove<InterfaceItEthernetService>();
            return;
        }

        if (Settings.FdsEthernetConnections is not null)
        {
            foreach (string fdsEthernetConnection in Settings.FdsEthernetConnections)
            {
                if (_inputOutputDevices.Any(x => x.Id == fdsEthernetConnection))
                {
                    continue;
                }

                InterfaceItEthernetService interfaceItEthernetService = new(fdsEthernetConnection);
                if (await interfaceItEthernetService.ConnectAsync(cancellationToken) == ConnectionStatus.Connected)
                {
                    _inputOutputDevices.Add(interfaceItEthernetService);
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
        if (Settings.FdsEthernetConnections?.Contains(connection) != false)
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
        string connection = await InterfaceItEthernetService.ReceiveControllerDiscoveryDataAsync();
        if (!string.IsNullOrEmpty(connection))
        {
            AddInterfaceItEthernetConnection(connection);
        }
    }

    #endregion
    
    #region FsCockpit

    [RelayCommand]
    private async Task ToggleFsCockpitAsync(CancellationToken cancellationToken)
    {
        if (!Settings.FsCockpit)
        {
            DisconnectAndRemove<FsCockpitServiceBase>();
            return;
        }

        IDeviceService? deviceService =  await FsCockpitServiceBase.GetFsCockpitService(cancellationToken);
        if (deviceService is not null)
        {
            _inputOutputDevices.Add(deviceService);
        }
    }
    
    #endregion
}