using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using DeviceInterfaceManager.Services.Devices;
using FluentAvalonia.UI.Controls;

namespace DeviceInterfaceManager.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel(HomeViewModel homeViewModel, ProfileCreatorViewModel profileCreatorViewModel, SettingsViewModel settingsViewModel, ObservableCollection<IDeviceService> inputOutputDevices)
    {
        HomeViewModel = homeViewModel;
        ProfileCreatorViewModel = profileCreatorViewModel;
        _settingsViewModel = settingsViewModel;
        InputOutputDevices = inputOutputDevices;
        InputOutputDevices.CollectionChanged += InputOutputDevicesOnCollectionChanged;
    }

    public HomeViewModel HomeViewModel { get; }

    public ProfileCreatorViewModel ProfileCreatorViewModel { get; }

    private readonly SettingsViewModel _settingsViewModel;

    [ObservableProperty]
    public partial ObservableCollection<IDeviceService> InputOutputDevices { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<DeviceViewModel> DeviceViewModels { get; set; } = [];

    [ObservableProperty]
    public partial ObservableObject? CurrentViewModel { get; set; }

    [ObservableProperty]
    public partial object? SelectedItem { get; set; }

    public async Task OnLoaded()
    {
        await _settingsViewModel.StartupAsync();
        if (_settingsViewModel.Settings.AutoHide)
        {
            await HomeViewModel.StartProfilesCommand.ExecuteAsync(null);
        }
    }

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

            case IDeviceService inputOutputDevice:
                DeviceViewModel? existingViewModel = DeviceViewModels.FirstOrDefault(vm => vm.DeviceService.Equals(inputOutputDevice));
                if (existingViewModel is null)
                {
                    DeviceViewModel newViewModel = new(inputOutputDevice);
                    CurrentViewModel = newViewModel;
                    DeviceViewModels.Add(newViewModel);
                    break;
                }

                CurrentViewModel = existingViewModel;
                break;
        }
    }

    private void InputOutputDevicesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action is not (NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Reset) || e.OldItems is null)
        {
            return;
        }

        foreach (IDeviceService inputOutputDevice in e.OldItems)
        {
            DeviceViewModel? viewModelToRemove = DeviceViewModels.FirstOrDefault(vm => vm.DeviceService.Equals(inputOutputDevice));
            if (viewModelToRemove is not null)
            {
                DeviceViewModels.Remove(viewModelToRemove);
            }
        }
    }
}