using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DeviceInterfaceManager.Models;

public sealed partial class Settings : ObservableObject
{
    [ObservableProperty]
    private bool _minimizedHide;

    [ObservableProperty]
    private bool _autoHide;

    [ObservableProperty]
    private bool _checkForUpdates;

    [ObservableProperty]
    private bool _server;

    [ObservableProperty]
    private string? _ipAddress;

    [ObservableProperty]
    private int? _port;

    [ObservableProperty]
    private bool _fdsUsb;

    [ObservableProperty]
    private bool _fdsEthernet;

    [ObservableProperty]
    private ObservableCollection<string>? _fdsEthernetConnections = [];

    public static Settings CreateSettings()
    {
        if (!File.Exists(App.SettingsFile))
        {
            return new Settings();
        }

        try
        {
            return JsonSerializer.Deserialize<Settings>(File.ReadAllText(App.SettingsFile)) ?? new Settings();
        }
        catch
        {
            return new Settings();
        }
    }

    private void SaveSettings()
    {
        string serialize = JsonSerializer.Serialize(this);
        File.WriteAllText(App.SettingsFile, serialize);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(FdsEthernetConnections) && FdsEthernetConnections is not null)
        {
            FdsEthernetConnections.CollectionChanged += (sender, args) => SaveSettings();
        }
        SaveSettings();
    }
}