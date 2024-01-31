using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using DeviceInterfaceManager.ViewModels;
using DeviceInterfaceManager.Views;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using HotAvalonia;
using Microsoft.Extensions.DependencyInjection;

namespace DeviceInterfaceManager;

public class App : Application
{
    public override void Initialize()
    {
        this.EnableHotReload();
        AvaloniaXamlLoader.Load(this);

        Ioc.Default.ConfigureServices(new ServiceCollection()
            .AddSingleton<IDialogService, DialogService>(provider => new DialogService(new DialogManager(new ViewLocator(), new DialogFactory().AddFluent()), provider.GetService))
            .AddSingleton<MainWindow>()
            .AddSingleton<MainWindowViewModel>()
            .AddSingleton<HomeViewModel>()
            .AddSingleton<ProfileCreatorViewModel>()
            .AddSingleton<SettingsViewModel>()
            .AddTransient<AskTextBoxViewModel>()
            .AddSingleton<ObservableCollection<DeviceItem>>()
            .BuildServiceProvider());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        DialogService.Show(null, MainWindowViewModel);
        GC.KeepAlive(typeof(DialogService));

        base.OnFrameworkInitializationCompleted();
    }

    public static MainWindowViewModel MainWindowViewModel => Ioc.Default.GetService<MainWindowViewModel>()!;
    private static IDialogService DialogService => Ioc.Default.GetService<IDialogService>()!;
    
    private void TrayIconClicked(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow!.WindowState = WindowState.Normal;
        }
    }

    private void MenuItemExitClick(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }
}