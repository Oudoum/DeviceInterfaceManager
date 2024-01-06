using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using CommunityToolkit.Mvvm.ComponentModel;
using DeviceInterfaceManager.Devices;
using DeviceInterfaceManager.Devices.interfaceIT.USB;
using FluentAvalonia.UI.Controls;

namespace DeviceInterfaceManager.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel()
    {
        if (Design.IsDesignMode)
        {
            DeviceItems.Add(new DeviceItem(new InterfaceItData()));
            return;
        }

        ExitCleanup();

        Startup();
    }

    [ObservableProperty]
    private ObservableCollection<DeviceItem> _deviceItems = [];

    [ObservableProperty]
    private ObservableObject? _currentViewModel;

    private readonly HomeViewModel _homeViewModel = new();

    //ProfileCreatorViewModel

    private readonly SettingsViewModel _settingsViewModel = new();

    [ObservableProperty]
    private object? _selectedItem;

    partial void OnSelectedItemChanged(object? value)
    {
        switch (value)
        {
            case NavigationViewItem navigationViewItem:
                switch (navigationViewItem.Content as string)
                {
                    case "Home":
                        CurrentViewModel = _homeViewModel;
                        break;

                    case "Profile Creator":
                        break;

                    case "Settings":
                        CurrentViewModel = _settingsViewModel;
                        break;
                }

                break;

            case DeviceItem categoryItem:
                CurrentViewModel = categoryItem.DeviceViewModel;
                break;
        }
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
    public string? Name { get; } = inputOutputDevice.BoardName;
    public string? ToolTip { get; } = inputOutputDevice.SerialNumber;
    public IconSource? Icon { get; } = (IconSource?)Application.Current!.FindResource(inputOutputDevice is IDeviceSerial ? "UsbPort" : "Ethernet");

    private DeviceViewModel? _deviceViewModel;
    public DeviceViewModel DeviceViewModel => _deviceViewModel ??= new DeviceViewModel(this);
}

public class MenuItemTemplateSelector : DataTemplateSelector
{
    [Content] public IDataTemplate? ItemTemplate { get; set; }

    protected override IDataTemplate? SelectTemplateCore(object item)
    {
        return ItemTemplate;
    }
}