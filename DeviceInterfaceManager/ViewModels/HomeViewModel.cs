﻿using System;
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

    [ObservableProperty]
    private ObservableCollection<ProfileMapping>? _deviceProfileList;

    private readonly List<Profile> _profiles = [];

    [ObservableProperty]
    private string? _aircraftTitle;

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

        if (DeviceProfileList is null && File.Exists(App.MappingsFile))
        {
            DeviceProfileList = JsonSerializer.Deserialize<ObservableCollection<ProfileMapping>>(File.ReadAllText(App.MappingsFile)) ?? throw new InvalidOperationException();

            DeviceProfileList.CollectionChanged += (sender, args) => DeviceProfileListHasChanged = true;

            foreach (ProfileMapping profileMapping in DeviceProfileList)
            {
                profileMapping.PropertyChanged += (sender, args) => DeviceProfileListHasChanged = true;
            }
        }

        string[] jsonFilePaths = Directory.GetFiles(App.ProfilesPath, "*.json");
        foreach (string filePath in jsonFilePaths)
        {
            if (!filePath.EndsWith(".json"))
            {
                return;
            }

            try
            {
                _profileCreatorModels.Add(JsonSerializer.Deserialize<ProfileCreatorModel>(File.ReadAllText(filePath)) ?? throw new InvalidOperationException());
            }
            catch
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

        DeviceProfileList?.Add(new ProfileMapping { DeviceName = "Device 1", ProfileName = "Profile 1" });
        DeviceProfileList?.Add(new ProfileMapping { DeviceName = "Device 2", ProfileName = "Profile 2" });
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
        DeviceProfileList ??= [];
        ProfileMapping profileMapping = new();
        profileMapping.PropertyChanged += (sender, args) => DeviceProfileListHasChanged = true;
        DeviceProfileList.Add(profileMapping);
    }

    [RelayCommand]
    private void Delete(ProfileMapping profileMapping)
    {
        DeviceProfileList?.Remove(profileMapping);
    }

    [ObservableProperty]
    private bool _deviceProfileListHasChanged;

    [RelayCommand]
    public void SaveMappings()
    {
        if (DeviceProfileList is null)
        {
            return;
        }

        string serialize = JsonSerializer.Serialize(DeviceProfileList);
        File.WriteAllText(App.MappingsFile, serialize);
        DeviceProfileListHasChanged = false;
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

            AircraftTitle = null;

            return;
        }

        AircraftTitle = await _simConnectClient.ConnectAsync(token);

        if (!token.IsCancellationRequested)
        {
            if (DeviceProfileList is null)
            {
                return;
            }

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