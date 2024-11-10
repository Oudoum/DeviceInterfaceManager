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
using DeviceInterfaceManager.Services;
using DeviceInterfaceManager.Services.Devices;
using Microsoft.Extensions.Logging;

namespace DeviceInterfaceManager.ViewModels;

public partial class HomeViewModel : ObservableRecipient
{
    private readonly ILogger _logger;
    private readonly SimConnectClientService _simConnectClientService;
    private readonly PmdgHelperService _pmdgHelperService;

    private readonly ObservableCollection<IDeviceService> _inputOutputDevices;
    private readonly ObservableCollection<ProfileCreatorModel> _profileCreatorModels = [];

    public IEnumerable<ProfileCreatorModel> FilteredProfileCreatorModels { get; private set; } = [];

    private IEnumerable<ProfileCreatorModel> GetFilteredProfileCreatorModels()
    {
        return IsFiltered ? _profileCreatorModels : _profileCreatorModels.Where(x => _inputOutputDevices.Any(y => y.DeviceName == x.DeviceName));
    }

    [ObservableProperty]
    private ObservableCollection<ProfileMapping>? _deviceProfileList;

    private readonly List<ProfileService> _profiles = [];

    [ObservableProperty]
    private string? _aircraftTitle;

    public HomeViewModel(ILogger<HomeViewModel> logger,SimConnectClientService simConnectClientService, PmdgHelperService pmdgHelperService, ObservableCollection<IDeviceService> inputOutputDevices)
    {
        _logger = logger;
        _simConnectClientService = simConnectClientService;
        _pmdgHelperService = pmdgHelperService;
        _simConnectClientService.AircraftTitleChanged += s => AircraftTitle = s; 
        
        _inputOutputDevices = inputOutputDevices;
        _inputOutputDevices.CollectionChanged += (_, _) =>
        {
            FilteredProfileCreatorModels = GetFilteredProfileCreatorModels();
            OnPropertyChanged(nameof(FilteredProfileCreatorModels));
        };
    }
    
#if DEBUG
    public HomeViewModel()
    {
        _logger = new LoggerFactory().CreateLogger<HomeViewModel>();
        _pmdgHelperService = new PmdgHelperService();
        _simConnectClientService = new SimConnectClientService(_pmdgHelperService ,Ioc.Default.GetRequiredService<SignalRClientService>());
        _inputOutputDevices =
        [
            new DeviceSerialService()
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

            DeviceProfileList.CollectionChanged += (_, _) => DeviceProfileListHasChanged = true;

            foreach (ProfileMapping profileMapping in DeviceProfileList)
            {
                profileMapping.PropertyChanged += (_, _) => DeviceProfileListHasChanged = true;
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
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred: {Message}", e.Message);
            }
        }
    }

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
        profileMapping.PropertyChanged += (_, _) => DeviceProfileListHasChanged = true;
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
            _simConnectClientService.Disconnect();

            foreach (ProfileService profile in _profiles)
            {
                await profile.DisposeAsync();
            }

            AircraftTitle = null;

            return;
        }

        AircraftTitle = await _simConnectClientService.ConnectAsync(token);

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
                
                if (!string.IsNullOrEmpty(AircraftTitle) && !string.IsNullOrEmpty(profileMapping.Aircraft) && !AircraftTitle.Contains(profileMapping.Aircraft))
                {
                    continue;
                }

                ProfileCreatorModel? profileCreatorModel = _profileCreatorModels.FirstOrDefault(x => x.ProfileName == profileMapping.ProfileName);

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
    }
}