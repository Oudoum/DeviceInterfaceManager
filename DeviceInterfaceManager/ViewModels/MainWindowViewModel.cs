using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using DeviceInterfaceManager.Models.Devices;
using FluentAvalonia.UI.Controls;

namespace DeviceInterfaceManager.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel(HomeViewModel homeViewModel, ProfileCreatorViewModel profileCreatorViewModel, SettingsViewModel settingsViewModel, ObservableCollection<IInputOutputDevice> inputOutputDevices)
    {
        HomeViewModel = homeViewModel;
        ProfileCreatorViewModel = profileCreatorViewModel;
        _settingsViewModel = settingsViewModel;
        InputOutputDevices = inputOutputDevices;
        InputOutputDevices.CollectionChanged += InputOutputDevicesOnCollectionChanged;
    }

#if DEBUG
    public MainWindowViewModel()
    {
        HomeViewModel = new HomeViewModel();
        ProfileCreatorViewModel = new ProfileCreatorViewModel();
        _settingsViewModel = new SettingsViewModel();
        InputOutputDevices = [];
    }
#endif

    public HomeViewModel HomeViewModel { get; }

    public ProfileCreatorViewModel ProfileCreatorViewModel { get; }

    private readonly SettingsViewModel _settingsViewModel;

    [ObservableProperty]
    private ObservableCollection<IInputOutputDevice> _inputOutputDevices;

    [ObservableProperty]
    private ObservableCollection<DeviceViewModel> _deviceViewModels = [];

    [ObservableProperty]
    private ObservableObject? _currentViewModel;

    [ObservableProperty]
    private object? _selectedItem;

    partial void OnSelectedItemChanged(object? value)
    {
        switch (value)
        {
            case NavigationViewItem navigationViewItem:
                CurrentViewModel = (navigationViewItem.Content as string) switch
                {
                    "Home" => HomeViewModel,
                    "Profile Creator" => ProfileCreatorViewModel,
                    "Settings" => _settingsViewModel,
                    _ => CurrentViewModel
                };
                break;

            case IInputOutputDevice inputOutputDevice:
                DeviceViewModel? existingViewModel = DeviceViewModels.FirstOrDefault(vm => vm.InputOutputDevice.Equals(inputOutputDevice));
                if (existingViewModel is null)
                {
                    DeviceViewModel newViewModel = new(inputOutputDevice);
                    CurrentViewModel = newViewModel;
                    DeviceViewModels.Add(newViewModel);
                    break;
                }

                CurrentViewModel = existingViewModel;
                break;

            default:
                CurrentViewModel = CurrentViewModel;
                break;
        }

        if (CurrentViewModel == HomeViewModel)
        {
            HomeViewModel.IsActive = true;
        }

        HomeViewModel.IsActive = false;
    }

    private void InputOutputDevicesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action is not (NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Reset) || e.OldItems is null)
        {
            return;
        }

        foreach (IInputOutputDevice inputOutputDevice in e.OldItems)
        {
            DeviceViewModel? viewModelToRemove = DeviceViewModels.FirstOrDefault(vm => vm.InputOutputDevice.Equals(inputOutputDevice));
            if (viewModelToRemove is not null)
            {
                DeviceViewModels.Remove(viewModelToRemove);
            }
        }
    }
}