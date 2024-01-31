using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Metadata;
using CommunityToolkit.Mvvm.ComponentModel;
using DeviceInterfaceManager.Models.Devices;
using FluentAvalonia.UI.Controls;
using InterfaceItData = DeviceInterfaceManager.Models.Devices.interfaceIT.USB.InterfaceItData;

namespace DeviceInterfaceManager.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel(HomeViewModel homeViewModel, ProfileCreatorViewModel profileCreatorViewModel,SettingsViewModel settingsViewModel, ObservableCollection<DeviceItem> deviceItems)
    {
        _homeViewModel = homeViewModel;
        _profileCreatorViewModel = profileCreatorViewModel;
        _settingsViewModel = settingsViewModel;
        _deviceItems = deviceItems;
        
        if (Design.IsDesignMode)
        {
            DeviceItems.Add(new DeviceItem(new DeviceSerialBase()));
            return;
        }

        ExitCleanup();

        Startup();
    }
    
    private readonly HomeViewModel _homeViewModel;

    private readonly ProfileCreatorViewModel _profileCreatorViewModel;

    private readonly SettingsViewModel _settingsViewModel;

    [ObservableProperty]
    private ObservableCollection<DeviceItem> _deviceItems;

    [ObservableProperty]
    private ObservableObject? _currentViewModel;

    [ObservableProperty]
    private object? _selectedItem;
    
    partial void OnSelectedItemChanged(object? value)
    {
        CurrentViewModel = value switch
        {
            NavigationViewItem navigationViewItem => (navigationViewItem.Content as string) switch
            {
                "Home" => _homeViewModel,
                "Profile Creator" => _profileCreatorViewModel,
                "Settings" => _settingsViewModel,
                _ => CurrentViewModel
            },
            DeviceItem categoryItem => categoryItem.DeviceViewModel,
            _ => CurrentViewModel
        };
    }

    private void Startup()
    {
        for (int i = 0; i < InterfaceItData.TotalControllers; i++)
        {
            InterfaceItData interfaceItData = new();
            interfaceItData.ConnectAsync();
            DeviceItems.Add(new DeviceItem(interfaceItData));
        }
    }

    private void ExitCleanup()
    {
        InterfaceItData.OpenControllers();
        if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownRequested += (_, _) =>
            {
                foreach (DeviceItem deviceItem in DeviceItems)
                {
                    deviceItem.InputOutputDevice.Disconnect();
                }

                InterfaceItData.CloseControllers();
            };
        }
    }
}

public class DeviceItem(IInputOutputDevice inputOutputDevice)
{
    public IInputOutputDevice InputOutputDevice { get; } = inputOutputDevice;
    public string? Name { get; } = inputOutputDevice.DeviceName;
    public string? ToolTip { get; } = inputOutputDevice.SerialNumber;
    public Geometry? Icon { get; } = (Geometry?)Application.Current!.FindResource(inputOutputDevice is IDeviceSerial ? "UsbPort" : "Ethernet");

    private DeviceViewModel? _deviceViewModel;
    public DeviceViewModel DeviceViewModel => _deviceViewModel ??= new DeviceViewModel(this);
}

public class MenuItemTemplateSelector : DataTemplateSelector
{
    [Content]
    public IDataTemplate? ItemTemplate { get; set; }

    protected override IDataTemplate? SelectTemplateCore(object item)
    {
        return ItemTemplate;
    }
}