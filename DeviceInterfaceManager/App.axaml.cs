using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.DependencyInjection;
using DeviceInterfaceManager.Server;
using DeviceInterfaceManager.Services;
using DeviceInterfaceManager.Services.Devices;
using DeviceInterfaceManager.ViewModels;
using DeviceInterfaceManager.ViewModels.Dialogs;
using DeviceInterfaceManager.Views;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using HotAvalonia;
using Microsoft.Extensions.Logging;

namespace DeviceInterfaceManager;

public class App : Application
{
    public static readonly string UserDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppDomain.CurrentDomain.FriendlyName);

    public static readonly string ProfilesPath = Path.Combine(UserDataPath, "Profiles");

    public static readonly string SettingsFile = Path.Combine(UserDataPath, "settings.json");

    public static readonly string MappingsFile = Path.Combine(UserDataPath, "mappings.json");

    public override void Initialize()
    {
        this.EnableHotReload();
        AvaloniaXamlLoader.Load(this);

        Directory.CreateDirectory(ProfilesPath);

        Ioc.Default.ConfigureServices(new ServiceCollection()
            .AddLogging(loggingBuilder => loggingBuilder.AddSerilog())
            .AddSingleton<IDialogService, DialogService>(provider =>
                new DialogService(
                    new DialogManager(
                        new ViewLocator(),
                        new DialogFactory().AddFluent(),
                        provider.GetService<ILogger<DialogManager>>()
                    ),
                    provider.GetService
                )
            )
            .AddSingleton<MainWindow>()
            .AddSingleton<MainWindowViewModel>()
            .AddSingleton<HomeViewModel>()
            .AddSingleton<ProfileCreatorViewModel>()
            .AddSingleton<SettingsViewModel>()
            .AddTransient<AskTextBoxDialogModel>()
            .AddTransient<AskComboBoxDialogModel>()
            .AddTransient<SelectSerialPortDialogModel>()
            .AddTransient<SelectInternetProtocolDialogModel>()
            .AddSingleton<ObservableCollection<IDeviceService>>()
            .AddSingleton<PmdgHelperService>()
            .AddSingleton<SimConnectClientService>()
            .AddSingleton<SignalRServerService>()
            .AddSingleton<SignalRClientService>()
            .BuildServiceProvider());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        BindingPlugins.DataValidators.RemoveAt(0);
        GC.KeepAlive(typeof(DialogService));

        CreateTrayIcon();

        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
            {
                if (IsApplicationAlreadyRunning())
                {
                    desktop.Shutdown();
                    return;
                }

                DialogService.Show(null, MainWindowViewModel);

                desktop.ShutdownRequested += (_, _) =>
                {
                    foreach (IDeviceService item in InputOutputDevices)
                    {
                        item.Disconnect();
                    }

                    MainWindowViewModel.HomeViewModel.SaveProfileMappings();
                };

                if (desktop.MainWindow is not null)
                {
                    desktop.MainWindow.PositionChanged += (_, _) =>
                    {
                        if (SettingsViewModel.Settings.MinimizedHide && desktop.MainWindow.WindowState == WindowState.Minimized)
                        {
                            HideWindow(desktop.MainWindow);
                        }
                    };

                    if (SettingsViewModel.Settings.MinimizedHide)
                    {
                    }

                    if (SettingsViewModel.Settings.AutoHide)
                    {
                        HideWindow(desktop.MainWindow);
                    }
                }

                break;
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static bool IsApplicationAlreadyRunning()
    {
        Assembly? assembly = Assembly.GetEntryAssembly();

        if (assembly is null)
        {
            return false;
        }

        string? assemblyName = assembly.GetName().Name;
        if (string.IsNullOrEmpty(assemblyName))
        {
            return false;
        }

        return Process.GetProcesses().Count(p => p.ProcessName.Contains(assemblyName)) > 1;
    }

    private void HideWindow(Window mainWindow)
    {
        mainWindow.WindowState = WindowState.Minimized;
        mainWindow.Hide();
        _trayIcon!.IsVisible = true;
        mainWindow.ShowInTaskbar = false;
    }

    public static MainWindowViewModel MainWindowViewModel => Ioc.Default.GetService<MainWindowViewModel>()!;
    private static SettingsViewModel SettingsViewModel => Ioc.Default.GetService<SettingsViewModel>()!;
    private static IDialogService DialogService => Ioc.Default.GetService<IDialogService>()!;
    private static ObservableCollection<IDeviceService> InputOutputDevices => Ioc.Default.GetService<ObservableCollection<IDeviceService>>()!;

    private TrayIcon? _trayIcon;

    private NativeMenuItem? _exitMenuItem;

    private void CreateTrayIcon()
    {
        _exitMenuItem = new NativeMenuItem("Exit");

        NativeMenu menu =
        [
            _exitMenuItem
        ];

        _trayIcon = new TrayIcon
        {
            ToolTipText = "Device Interface Manager",
            Icon = new WindowIcon(AssetLoader.Open(new Uri("avares://DeviceInterfaceManager/Assets/DIM.ico"))),
            IsVisible = false,
            Menu = menu
        };


        _exitMenuItem.Click += MenuItemExitClick;
        _trayIcon.Clicked += TrayIconClicked;
    }

    private void TrayIconClicked(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime { MainWindow: not null } desktop)
        {
            return;
        }

        desktop.MainWindow.WindowState = WindowState.Normal;
        desktop.MainWindow.ShowInTaskbar = true;
        if (_trayIcon is not null)
        {
            _trayIcon.IsVisible = false;
        }

        desktop.MainWindow.Show();
    }

    private void MenuItemExitClick(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }
}