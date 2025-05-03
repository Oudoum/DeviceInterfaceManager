using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceInterfaceManager.Models;
using DeviceInterfaceManager.Server;
using DeviceInterfaceManager.Services;
using DeviceInterfaceManager.Services.Devices;
using DeviceInterfaceManager.Services.Devices.Fds;
using DeviceInterfaceManager.Services.Devices.FsCockpit;
using DeviceInterfaceManager.ViewModels.Dialogs;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using Microsoft.Extensions.Logging;
using Velopack;
using Velopack.Sources;
using InterfaceItUsbService = DeviceInterfaceManager.Services.Devices.Fds.InterfaceItUsbService;

namespace DeviceInterfaceManager.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ILogger _logger;
    private readonly ObservableCollection<IDeviceService> _inputOutputDevices;
    private readonly SignalRServerService _signalRServerService;
    private readonly SignalRClientService _signalRClientService;
    private readonly IDialogService _dialogService;

    public SettingsViewModel(ILogger<SettingsViewModel> logger, ObservableCollection<IDeviceService> inputOutputDevices, SignalRServerService signalRServerService, SignalRClientService signalRClientService, IDialogService dialogService)
    {
        _logger = logger;
        _inputOutputDevices = inputOutputDevices;
        _signalRServerService = signalRServerService;
        _signalRClientService = signalRClientService;
        _dialogService = dialogService;
    }

    public Settings Settings { get; } = Settings.CreateSettings();

    public async Task StartupAsync()
    {
        UpdateManager updateManager = CreateUpdateManager();

        CurrentVersion = updateManager.CurrentVersion?.ToFullString();

        if (Settings.CheckForUpdates)
        {
            await CheckForUpdatesCommand.ExecuteAsync(updateManager);
        }

        if (Settings.FdsUsb)
        {
            await ToggleFdsUsbCommand.ExecuteAsync(null);
        }

        if (Settings.FdsEthernet)
        {
            await ToggleFdsEthernetCommand.ExecuteAsync(null);
        }

        if (Settings.FsCockpit)
        {
            await ToggleFsCockpitCommand.ExecuteAsync(null);
        }

        if (Settings.Server)
        {
            await StartServerCommand.ExecuteAsync(null);
        }
    }

    [ObservableProperty]
    private string? _currentVersion;

    private UpdateManager CreateUpdateManager()
    {
        GithubSource githubSource = new("https://github.com/Oudoum/DeviceInterfaceManager", null, false);
        return new UpdateManager(githubSource, logger: _logger);
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
            UpdateInfo? updateInfo = await updateManager.CheckForUpdatesAsync().ConfigureAwait(true);

            if (updateInfo is null)
            {
                IsUpToDate = true;
                return;
            }

            await updateManager.DownloadUpdatesAsync(updateInfo).ConfigureAwait(true);

            updateManager.ApplyUpdatesAndRestart(updateInfo);
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
    private async Task StartServerAsync(CancellationToken cancellationToken = default)
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
    private async Task ToggleFdsUsbAsync(CancellationToken cancellationToken = default)
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
    private async Task ToggleFdsEthernetAsync(CancellationToken cancellationToken = default)
    {
        if (!Settings.FdsEthernet)
        {
            DisconnectAndRemove<InterfaceItEthernetService>();
            return;
        }

        if (Settings.Connections is null)
        {
            return;
        }

        foreach (IConnection connection in Settings.Connections)
        {
            if (connection is not Connection interfaceItEthernetConnection ||
                string.IsNullOrEmpty(interfaceItEthernetConnection.DriverName) ||
                string.IsNullOrWhiteSpace(interfaceItEthernetConnection.ConnectionName) ||
                interfaceItEthernetConnection.DriverName != ProfileCreatorModel.FdsEnet)
            {
                continue;
            }

            InterfaceItEthernetService interfaceItEthernetService = new(connection.ConnectionName);
            if (await interfaceItEthernetService.ConnectAsync(cancellationToken) == ConnectionStatus.Connected)
            {
                _inputOutputDevices.Add(interfaceItEthernetService);
            }
        }
    }

    [RelayCommand]
    private async Task ChangeInterfaceItEthernetDeviceAsync()
    {
        await ChangeInternetProtocolDeviceAsync(ProfileCreatorModel.FdsEnet);
    }

    #endregion

    #region FsCockpit

    [RelayCommand]
    private async Task ToggleFsCockpitAsync(CancellationToken cancellationToken = default)
    {
        if (!Settings.FsCockpit)
        {
            DisconnectAndRemove<FsCockpitServiceBase>();
            return;
        }

        if (Settings.Connections is null)
        {
            return;
        }

        foreach (IConnection connection in Settings.Connections)
        {
            if (connection is not FsCockpitConnection fsCockpitConnection ||
                string.IsNullOrEmpty(fsCockpitConnection.DriverName) ||
                string.IsNullOrWhiteSpace(fsCockpitConnection.ConnectionName) ||
                fsCockpitConnection.DriverName != ProfileCreatorModel.FsCockpit)
            {
                continue;
            }

            FsCockpitSerialPortService fsCockpitService = new(fsCockpitConnection.ConnectionName, fsCockpitConnection.HasHighTensionDetents);
            IDeviceService? deviceService = await fsCockpitService.GetDeviceServiceAsync(cancellationToken);
            if (deviceService is not null)
            {
                _inputOutputDevices.Add(deviceService);
            }
        }
    }

    [RelayCommand]
    private async Task ChangeFsCockpitDeviceAsync()
    {
        await ChangeSerialPortDeviceAsync(ProfileCreatorModel.FsCockpit);
    }

    #endregion

    private void AddDevices(IEnumerable<IDeviceService>? devices)
    {
        if (devices is null)
        {
            return;
        }

        foreach (IDeviceService device in devices)
        {
            _inputOutputDevices.Add(device);
        }
    }

    private async Task ChangeSerialPortDeviceAsync(string driverName)
    {
        SelectSerialPortDialogModel dialogModel = _dialogService.CreateViewModel<SelectSerialPortDialogModel>();
        dialogModel.DriverName = driverName;
        dialogModel.Text = "Select a serial port.";
        if (Settings.Connections is not null)
        {
            ObservableCollection<IConnection> connections = [];
            foreach (IConnection connection in Settings.Connections)
            {
                connections.Add(connection);
            }

            dialogModel.Connections = connections;
        }

        ContentDialogResult result = await _dialogService.ShowContentDialogAsync(App.MainWindowViewModel, new ContentDialogSettings
        {
            Content = dialogModel,
            Title = "COM Connections",
            PrimaryButtonText = "OK",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary
        });

        if (result == ContentDialogResult.Primary)
        {
            Settings.Connections = dialogModel.Connections;
        }
    }

    private async Task ChangeInternetProtocolDeviceAsync(string driverName)
    {
        SelectInternetProtocolDialogModel dialogModel = _dialogService.CreateViewModel<SelectInternetProtocolDialogModel>();
        dialogModel.DriverName = driverName;
        dialogModel.Text = "Add an IP-Address.";
        if (Settings.Connections is not null)
        {
            ObservableCollection<IConnection> connections = [];
            foreach (IConnection connection in Settings.Connections)
            {
                connections.Add(connection);
            }

            dialogModel.Connections = connections;
        }

        ContentDialogResult result = await _dialogService.ShowContentDialogAsync(App.MainWindowViewModel, new ContentDialogSettings
        {
            Content = dialogModel,
            Title = "IP Connections",
            PrimaryButtonText = "OK",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary
        });

        if (result == ContentDialogResult.Primary)
        {
            Settings.Connections = dialogModel.Connections;
        }
    }
}