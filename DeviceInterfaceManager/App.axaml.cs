using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.DependencyInjection;
using DeviceInterfaceManager.Models.Devices;
using DeviceInterfaceManager.ViewModels;
using DeviceInterfaceManager.Views;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WritableJsonConfiguration;

namespace DeviceInterfaceManager;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        Ioc.Default.ConfigureServices(new ServiceCollection()
            .AddSingleton<IDialogService, DialogService>(provider => new DialogService(new DialogManager(new ViewLocator(), new DialogFactory().AddFluent()), provider.GetService))
            .AddSingleton<IConfiguration>(WritableJsonConfigurationFabric.Create("settings.json"))
            .AddSingleton<MainWindow>()
            .AddSingleton<MainWindowViewModel>()
            .AddSingleton<HomeViewModel>()
            .AddSingleton<ProfileCreatorViewModel>()
            .AddSingleton<SettingsViewModel>()
            .AddTransient<AskTextBoxViewModel>()
            .AddTransient<AskComboBoxViewModel>()
            .AddSingleton<ObservableCollection<IInputOutputDevice>>()
            .BuildServiceProvider());
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);
        GC.KeepAlive(typeof(DialogService));
        
        DialogService.Show(null, MainWindowViewModel);

        CreateTrayIcon();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: not null } desktop)
        {
            desktop.ShutdownRequested += (_, _) =>
            {
                foreach (IInputOutputDevice item in InputOutputDevices)
                {
                    item.Disconnect();
                }
            };
            
            desktop.MainWindow.PositionChanged += (sender, args) =>
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

        if (!Design.IsDesignMode)
        {
            await SettingsViewModel.Startup();
        }
        
        base.OnFrameworkInitializationCompleted();
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
    private static ObservableCollection<IInputOutputDevice> InputOutputDevices => Ioc.Default.GetService<ObservableCollection<IInputOutputDevice>>()!;
    
    //TrayIcon
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
        _trayIcon!.IsVisible = false;
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