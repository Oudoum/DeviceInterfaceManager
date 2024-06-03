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
    private bool _isP3D;

    [ObservableProperty]
    private bool _fdsUsb;
    
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
        
        SaveSettings();
    }
}