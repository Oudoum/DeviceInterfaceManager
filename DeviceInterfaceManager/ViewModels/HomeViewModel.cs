using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceInterfaceManager.Models;
using DeviceInterfaceManager.Services;
using DeviceInterfaceManager.Services.Devices;

using Microsoft.Extensions.Logging;

namespace DeviceInterfaceManager.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly ILogger _logger;
    private readonly SimConnectClientService _simConnectClientService;
    private readonly PmdgHelperService _pmdgHelperService;
    private readonly ObservableCollection<IDeviceService> _inputOutputDevices;

    private readonly ObservableCollection<Tuple<string, ProfileCreatorModel>> _profileCreatorModelsByFileName = [];
    private readonly List<ProfileService> _profiles = [];
    private readonly FileSystemWatcher _watcher;

    [ObservableProperty]
    private IEnumerable<Tuple<string, ProfileCreatorModel>> _filteredProfileCreatorModelsByFileName = [];

    private IEnumerable<Tuple<string, ProfileCreatorModel>> GetFilteredProfileCreatorModels()
    {
        return _profileCreatorModelsByFileName.Where(x => _inputOutputDevices.Any(y => y.DeviceName == x.Item2.DeviceName));
    }

    [ObservableProperty]
    private ObservableCollection<ProfileMapping> _profileMappings = [];

    [ObservableProperty]
    private string? _aircraftTitle;


    public HomeViewModel(ILogger<HomeViewModel> logger, SimConnectClientService simConnectClientService, PmdgHelperService pmdgHelperService, ObservableCollection<IDeviceService> inputOutputDevices, SettingsViewModel settingsViewModel)
    {
        _logger = logger;
        _simConnectClientService = simConnectClientService;
        _pmdgHelperService = pmdgHelperService;
        _simConnectClientService.AircraftTitleChanged += title => AircraftTitle = title;

        _inputOutputDevices = inputOutputDevices;
        _inputOutputDevices.CollectionChanged += (_, _) => { FilteredProfileCreatorModelsByFileName = GetFilteredProfileCreatorModels(); };

        ReadAllProfiles();
        
        _watcher = new FileSystemWatcher(App.ProfilesPath, "*.json");
        _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
        _watcher.EnableRaisingEvents = true;
        EnableFileSystemWatcher();
    }

    private void EnableFileSystemWatcher()
    {
        _watcher.Changed += OnChanged;
        _watcher.Created += OnChanged;
        _watcher.Deleted += OnChanged;
        _watcher.Renamed += OnRenamed;
    }

    private void DisableFileSystemWatcher()
    {
        _watcher.Renamed -= OnRenamed;
        _watcher.Deleted -= OnChanged;
        _watcher.Created -= OnChanged;
        _watcher.Changed -= OnChanged;
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        switch (e.ChangeType)
        {
            case WatcherChangeTypes.Created:
                AddProfile(e.FullPath);
                break;

            case WatcherChangeTypes.Deleted:
                ReplaceProfilesCreatorModels(e.FullPath, remove:true);
                break;

            case WatcherChangeTypes.Changed:
                ReplaceProfilesCreatorModels(e.FullPath);
                break;
        }
        
        FilteredProfileCreatorModelsByFileName = GetFilteredProfileCreatorModels();
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.OldName) || string.IsNullOrEmpty(e.Name))
        {
            return;
        }

        if (!e.Name.EndsWith(".json"))
        {
            AddProfile(e.OldFullPath);
            return;
        }

        string name = Path.GetFileNameWithoutExtension(e.Name);
        ReplaceProfilesCreatorModels(e.OldFullPath, name);
    }

    private ProfileCreatorModel? DeserializeProfileCreatorModel(string fullPath)
    {
        string? allText = null;

        for (int i = 0; i < 3; i++)
        {
            try
            {
                allText = File.ReadAllText(fullPath);
                break;
            }
            catch (IOException)
            {
            }
        }

        if (allText is null)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<ProfileCreatorModel>(allText);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred: {Message}", e.Message);
            return null;
        }
    }

    private static string GetProfileName(string fullPath)
    {
        return Path.GetFileNameWithoutExtension(fullPath);
    }

    private void AddProfile(string fullPath)
    {
        string name = GetProfileName(fullPath);
        ProfileCreatorModel? profileCreatorModel = DeserializeProfileCreatorModel(fullPath);

        if (profileCreatorModel is not null)
        {
            _profileCreatorModelsByFileName.Add(new Tuple<string, ProfileCreatorModel>(name, profileCreatorModel));
        }
    }

    private void ReplaceProfilesCreatorModels(string fullPath, string? newName = null, bool remove = false)
    {
        string oldName = Path.GetFileNameWithoutExtension(fullPath);

        var tuple = _profileCreatorModelsByFileName.FirstOrDefault(x => x.Item1 == oldName);
        if (tuple is null)
        {
            return;
        }

        if (remove)
        {
            _profileCreatorModelsByFileName.Remove(tuple);
        }

        int index = _profileCreatorModelsByFileName.IndexOf(tuple);
        if (index == -1)
        {
            return;
        }

        ProfileCreatorModel? profileCreatorModel = tuple.Item2;

        if (string.IsNullOrEmpty(newName))
        {
            profileCreatorModel = DeserializeProfileCreatorModel(fullPath);
        }

        if (profileCreatorModel is null)
        {
            return;
        }

        newName ??= oldName;
        _profileCreatorModelsByFileName[index] = new Tuple<string, ProfileCreatorModel>(newName, profileCreatorModel);
        FilteredProfileCreatorModelsByFileName = GetFilteredProfileCreatorModels();
    }

    private void ReadAllProfiles()
    {
        if (!Directory.Exists(App.ProfilesPath))
        {
            return;
        }

        if (File.Exists(App.MappingsFile))
        {
            ObservableCollection<ProfileMapping>? profileMappings = null;
            try
            {
                string allText = File.ReadAllText(App.MappingsFile);
                profileMappings = JsonSerializer.Deserialize<ObservableCollection<ProfileMapping>>(allText);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred: {Message}", e.Message);
            }

            if (profileMappings is not null)
            {
                ProfileMappings = profileMappings;
                ProfileMappings.CollectionChanged += (_, _) => DeviceProfileListHasChanged = true;

                OnProfileMappingPropertyChanged(ProfileMappings[^1], new PropertyChangedEventArgs(null));

                foreach (ProfileMapping profileMapping in ProfileMappings)
                {
                    profileMapping.PropertyChanged += OnProfileMappingPropertyChanged;
                }
            }
        }

        if (ProfileMappings.Count == 0)
        {
            AddProfileMapping();
        }

        string[] jsonFilePaths = Directory.GetFiles(App.ProfilesPath, "*.json");
        foreach (string filePath in jsonFilePaths)
        {
            AddProfile(filePath);
        }

        FilteredProfileCreatorModelsByFileName = GetFilteredProfileCreatorModels();
    }

    private void OnProfileMappingPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not ProfileMapping changedProfileMapping)
        {
            return;
        }

        if (ProfileMappings[^1] != changedProfileMapping &&
            string.IsNullOrEmpty(changedProfileMapping.DeviceName) &&
            string.IsNullOrEmpty(changedProfileMapping.Id) &&
            string.IsNullOrEmpty(changedProfileMapping.Aircraft))
        {
            RemoveProfileMapping(changedProfileMapping);
            DeviceProfileListHasChanged = true;
            return;
        }

        if (ProfileMappings[^1] != changedProfileMapping)
        {
            return;
        }

        if (string.IsNullOrEmpty(changedProfileMapping.DeviceName) &&
            string.IsNullOrEmpty(changedProfileMapping.Id) &&
            string.IsNullOrEmpty(changedProfileMapping.Aircraft))
        {
            return;
        }

        AddProfileMapping();
        DeviceProfileListHasChanged = true;
    }

    private void AddProfileMapping()
    {
        ProfileMapping profileMapping = new();
        profileMapping.PropertyChanged += OnProfileMappingPropertyChanged;
        ProfileMappings.Add(profileMapping);
    }

    private void RemoveProfileMapping(ProfileMapping profileMapping)
    {
        ProfileMappings.Remove(profileMapping);
    }

    [ObservableProperty]
    private bool _deviceProfileListHasChanged;

    [RelayCommand]
    public void SaveProfileMappings()
    {
        string serialize = JsonSerializer.Serialize(ProfileMappings);
        File.WriteAllText(App.MappingsFile, serialize);
        DeviceProfileListHasChanged = false;
    }

    [ObservableProperty]
    private bool _isStarted;

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task StartProfilesAsync(CancellationToken token = default)
    {
        IsStarted = !IsStarted;

        if (!IsStarted)
        {
            _simConnectClientService.Disconnect();

            foreach (ProfileService profile in _profiles)
            {
                await profile.DisposeAsync();
            }

            AircraftTitle = null;

            EnableFileSystemWatcher();
            return;
        }

        DisableFileSystemWatcher();
        AircraftTitle = await _simConnectClientService.ConnectAsync(token);

        if (!token.IsCancellationRequested)
        {
            foreach (ProfileMapping profileMapping in ProfileMappings)
            {
                if (!profileMapping.IsActive)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(AircraftTitle) && !string.IsNullOrEmpty(profileMapping.Aircraft) && !AircraftTitle.Contains(profileMapping.Aircraft))
                {
                    continue;
                }

                ProfileCreatorModel? profileCreatorModel = _profileCreatorModelsByFileName.FirstOrDefault(x => x.Item1 == profileMapping.ProfileName)?.Item2;

                if (profileCreatorModel is null)
                {
                    continue;
                }

                IDeviceService? inputOutputDevice = _inputOutputDevices.FirstOrDefault(x => x.Id == profileMapping.Id);

                if (inputOutputDevice is null)
                {
                    continue;
                }

                ProfileService profileService = new(_simConnectClientService, _pmdgHelperService, profileCreatorModel, inputOutputDevice);
                _profiles.Add(profileService);
            }

            return;
        }

        IsStarted = !IsStarted;
        EnableFileSystemWatcher();
    }
}