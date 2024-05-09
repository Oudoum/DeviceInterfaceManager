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

public partial class HomeViewModel : ObservableObject
{
    private readonly SimConnectClient _simConnectClient;
    
    private readonly ObservableCollection<IInputOutputDevice> _inputOutputDevices;

    private List<ProfileCreatorModel> _profileCreatorModels = [];

    private List<Profile> _profiles = [];

    public HomeViewModel(SimConnectClient simConnectClient ,ObservableCollection<IInputOutputDevice> inputOutputDevices)
    {
        _simConnectClient = simConnectClient;
        _inputOutputDevices = inputOutputDevices;

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
    
    public HomeViewModel()
    {
        _simConnectClient = Ioc.Default.GetService<SimConnectClient>()!;
        _inputOutputDevices = [];
    }

    [RelayCommand]
    private async Task StartProfile()
    {
        await _simConnectClient.ConnectAsync(new CancellationToken());
        // ProfileCreatorModel profileCreatorModel = _profileCreatorModels.First();
        // _simConnectClient.Helper?.Init(profileCreatorModel);
        // Profile profile = new(_simConnectClient, profileCreatorModel, _inputOutputDevices.First());
        // _profiles.Add(profile);
    }
}