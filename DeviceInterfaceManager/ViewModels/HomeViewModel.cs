using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using DeviceInterfaceManager.Models;
using DeviceInterfaceManager.Models.Devices;
using DeviceInterfaceManager.Models.FlightSim;
using DeviceInterfaceManager.Models.FlightSim.MSFS;

namespace DeviceInterfaceManager.ViewModels;

public partial class HomeViewModel : ObservableRecipient
{
    private readonly SimConnectClient _simConnectClient;

    public ObservableCollection<IInputOutputDevice> InputOutputDevices { get; }

    private readonly ObservableCollection<ProfileCreatorModel> _profileCreatorModels = [];

    public IEnumerable<ProfileCreatorModel> FilteredProfileCreatorModels { get; private set; } = [];

    private IEnumerable<ProfileCreatorModel> GetFilteredProfileCreatorModels()
    {
        return IsFiltered ? _profileCreatorModels : _profileCreatorModels.Where(x => InputOutputDevices.Any(y => y.DeviceName == x.DeviceName));
    }

    public ObservableCollection<ProfileMapping> DeviceProfileList { get; } = [];

    private readonly List<Profile> _profiles = [];

    public HomeViewModel(SimConnectClient simConnectClient, ObservableCollection<IInputOutputDevice> inputOutputDevices)
    {
        _simConnectClient = simConnectClient;
        InputOutputDevices = inputOutputDevices;

        InputOutputDevices.CollectionChanged += (sender, args) =>
        {
            FilteredProfileCreatorModels = GetFilteredProfileCreatorModels();
            OnPropertyChanged(nameof(FilteredProfileCreatorModels));
        };
    }

    protected override void OnActivated()
    {
        _profileCreatorModels.Clear();

        if (!Directory.Exists(App.ProfilesPath))
        {
            return;
        }

        foreach (string filePath in Directory.GetFiles(App.ProfilesPath))
        {
            try
            {
                _profileCreatorModels.Add(JsonSerializer.Deserialize<ProfileCreatorModel>(File.ReadAllText(filePath)) ?? throw new InvalidOperationException());
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }

#if DEBUG
    public HomeViewModel()
    {
        _simConnectClient = Ioc.Default.GetService<SimConnectClient>()!;
        InputOutputDevices =
        [
            new DeviceSerialBase()
        ];

        _profileCreatorModels =
        [
            new ProfileCreatorModel { DeviceName = "Device 1", ProfileName = "Profile 1" },
            new ProfileCreatorModel { DeviceName = "Device 2", ProfileName = "Profile 2" }
        ];

        DeviceProfileList.Add(new ProfileMapping { DeviceName = "Device 1", ProfileName = "Profile 1" });
        DeviceProfileList.Add(new ProfileMapping { DeviceName = "Device 2", ProfileName = "Profile 2" });
    }
#endif
    
    private bool _isFiltered;
    public bool IsFiltered
    {
        get => _isFiltered;
        set
        {
            _isFiltered = value;
            FilteredProfileCreatorModels = GetFilteredProfileCreatorModels();
            OnPropertyChanged(nameof(FilteredProfileCreatorModels));
        }
    }

    [RelayCommand]
    private void Add()
    {
        DeviceProfileList.Add(new ProfileMapping());
    }

    [RelayCommand]
    private void Delete(ProfileMapping profileMapping)
    {
        DeviceProfileList.Remove(profileMapping);
    }

    [ObservableProperty]
    private bool _isStarted;

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task StartProfilesAsync(CancellationToken token)
    {
        IsStarted = !IsStarted;

        if (!IsStarted)
        {
            _simConnectClient.Disconnect();

            foreach (Profile profile in _profiles)
            {
                await profile.DisposeAsync();
            }

            return;
        }

        await _simConnectClient.ConnectAsync(token);

        if (!token.IsCancellationRequested)
        {
            foreach (ProfileMapping profileMapping in DeviceProfileList)
            {
                if (!profileMapping.IsActive)
                {
                    continue;
                }

                ProfileCreatorModel? profileCreatorModel = _profileCreatorModels.FirstOrDefault(x => x.ProfileName == profileMapping.ProfileName);

                if (profileCreatorModel is null)
                {
                    continue;
                }

                IInputOutputDevice? inputOutputDevice = InputOutputDevices.FirstOrDefault(x => x.Id == profileMapping.Id);

                if (inputOutputDevice is null)
                {
                    continue;
                }

                Profile profile = new(_simConnectClient, profileCreatorModel, inputOutputDevice);
                _profiles.Add(profile);
            }

            return;
        }

        IsStarted = !IsStarted;
    }
}