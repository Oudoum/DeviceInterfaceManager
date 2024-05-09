using System.ComponentModel;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DeviceInterfaceManager.Models;

public sealed partial class Settings : ObservableObject
{
    private static readonly string SettingsFile = Path.Combine(App.UserDataPath, "settings.json");

    [ObservableProperty]
    private bool _minimizedHide;

    [ObservableProperty]
    private bool _autoHide;

    [ObservableProperty]
    private bool _isP3D;

    [ObservableProperty]
    private bool _fdsUsb;
    
    public static Settings CreateSettings()
    {
        if (!File.Exists(SettingsFile))
        {
            return new Settings();
        }

        try
        {
            return JsonSerializer.Deserialize<Settings>(File.ReadAllText(SettingsFile)) ?? new Settings();
        }
        catch
        {
            return new Settings();
        }
    }

    private void SaveSettings()
    {
        string serialize = JsonSerializer.Serialize(this);
        File.WriteAllText(SettingsFile, serialize);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        
        SaveSettings();
    }
}