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

    private CancellationTokenSource? _fdsUsbCts;
    private CancellationTokenSource? _fdsEnetCts;
    private CancellationTokenSource? _fsCockpitCts;

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

        if (Settings.Server)
        {
            await StartServerAsync();
        }

        if (Settings.CheckForUpdates)
        {
            await CheckForUpdatesAsync(updateManager);
        }

        if (Settings.FdsUsb)
        {
            await ToggleFdsUsbAsync();
        }

        if (Settings.FdsEthernet)
        {
            await ToggleFdsEthernetAsync();
        }

        if (Settings.FsCockpit)
        {
            await ToggleFsCockpitAsync();
        }
    }

    public void Disconnect()
    {
        _fdsUsbCts?.Cancel();
        _fdsEnetCts?.Cancel();
        _fsCockpitCts?.Cancel();
    }

    [ObservableProperty]
    public partial string? CurrentVersion { get; set; }

    private static UpdateManager CreateUpdateManager()
    {
        GithubSource githubSource = new("https://github.com/Oudoum/DeviceInterfaceManager", null, false);
        return new UpdateManager(githubSource);
    }

    [ObservableProperty]
    public partial bool IsUpToDate { get; set; }

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
    public partial string? WasmModuleUpdaterMessage { get; set; }

    [RelayCommand]
    private async Task UpdateDimWasmModuleAsync() => WasmModuleUpdaterMessage = await WasmModuleUpdateService.Create().InstallWasmModule();

    [RelayCommand]
    private static void OpenUserDataFolder() => Process.Start("explorer.exe", App.UserDataPath);

    private async Task DisconnectAndRemove<T>(CancellationTokenSource? cancellationTokenSource) where T : IDeviceService
    {
        if (cancellationTokenSource is not null)
        { 
            await cancellationTokenSource.CancelAsync();
        }

        foreach (IDeviceService inputOutputDevice in _inputOutputDevices.ToArray())
        {
            if (inputOutputDevice is not T)
            {
                continue;
            }

            await inputOutputDevice.Disconnect();
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

    [RelayCommand]
    private async Task ToggleFdsUsbAsync()
    {
        if (!Settings.FdsUsb)
        {
            await DisconnectAndRemove<InterfaceItUsbService>(_fdsUsbCts);
            return;
        }

        _fdsUsbCts = new CancellationTokenSource();
        for (int i = 0; i < InterfaceItUsbService.TotalControllers; i++)
        {
            InterfaceItUsbService interfaceItUsbService = new();
            if (await interfaceItUsbService.ConnectAsync(_fdsUsbCts.Token) == ConnectionStatus.Connected)
            {
                _inputOutputDevices.Add(interfaceItUsbService);
            }
        }
    }

    [RelayCommand]
    private async Task ToggleFdsEthernetAsync()
    {
        if (!Settings.FdsEthernet)
        {
            await DisconnectAndRemove<InterfaceItEthernetService>(_fdsEnetCts);
            return;
        }

        if (Settings.Connections is null)
        {
            return;
        }

        _fdsEnetCts = new CancellationTokenSource();
        foreach (IConnection connection in Settings.Connections)
        {
            if (connection is not Connection interfaceItEthernetConnection ||
                string.IsNullOrEmpty(interfaceItEthernetConnection.DriverName) ||
                string.IsNullOrWhiteSpace(interfaceItEthernetConnection.ConnectionName) ||
                interfaceItEthernetConnection.DriverName != ProfileCreatorModel.FdsEnet)
            {
                continue;
            }

            InterfaceItEthernetService interfaceItEthernetService = new(connection.ConnectionName, _logger);
            if (await interfaceItEthernetService.ConnectAsync(_fdsEnetCts.Token) == ConnectionStatus.Connected)
            {
                _inputOutputDevices.Add(interfaceItEthernetService);
            }
        }
    }

    [RelayCommand]
    private async Task ChangeInterfaceItEthernetDeviceAsync() => await ChangeInternetProtocolDeviceAsync(ProfileCreatorModel.FdsEnet);

    [RelayCommand]
    private async Task ToggleFsCockpitAsync()
    {
        if (!Settings.FsCockpit)
        {
            await DisconnectAndRemove<FsCockpitServiceBase>(_fsCockpitCts);
            return;
        }

        if (Settings.Connections is null)
        {
            return;
        }

        _fsCockpitCts = new CancellationTokenSource();
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
            IDeviceService? deviceService = await fsCockpitService.GetDeviceServiceAsync(_fsCockpitCts.Token);
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

    private async Task ChangeDeviceAsync<TSelectConnectionDialogModel>(string driverName, string title, string text) where TSelectConnectionDialogModel : SelectConnectionDialogModel
    {
        TSelectConnectionDialogModel dialogModel = _dialogService.CreateViewModel<TSelectConnectionDialogModel>();
        dialogModel.DriverName = driverName;
        dialogModel.Text = text;

        if (Settings.Connections is not null)
        {
            dialogModel.Connections = new ObservableCollection<IConnection>(Settings.Connections);
        }

        ContentDialogResult result = await _dialogService.ShowContentDialogAsync(App.MainWindowViewModel, new ContentDialogSettings
        {
            Content = dialogModel,
            Title = title,
            PrimaryButtonText = "OK",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary
        });

        if (result == ContentDialogResult.Primary)
        {
            Settings.Connections = dialogModel.Connections;
        }
    }

    private async Task ChangeSerialPortDeviceAsync(string driverName)
    {
        await ChangeDeviceAsync<SelectSerialPortDialogModel>(driverName, "COM Connections", "Select a serial port.");
    }
 
    private async Task ChangeInternetProtocolDeviceAsync(string driverName)
    {
        await ChangeDeviceAsync<SelectInternetProtocolDialogModel>(driverName, "IP Connections", "Add an IP-Address.");
    }
}