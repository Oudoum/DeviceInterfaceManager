using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DeviceInterfaceManager.Models;

public partial class Settings : ObservableObject
{
    [ObservableProperty]
    public partial bool MinimizedHide { get; set; }

    [ObservableProperty]
    public partial bool AutoHide { get; set; }

    [ObservableProperty]
    public partial bool CheckForUpdates { get; set; }

    [ObservableProperty]
    public partial bool Server { get; set; }

    [ObservableProperty]
    public partial string? IpAddress { get; set; }

    [ObservableProperty]
    public partial int? Port { get; set; }

    [ObservableProperty]
    public partial bool FdsUsb { get; set; }

    [ObservableProperty]
    public partial bool FdsEthernet { get; set; }

    [ObservableProperty]
    public partial bool FsCockpit { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<IConnection>? Connections { get; set; } = [];

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

        if (e.PropertyName == nameof(Connections) && Connections is not null)
        {
            Connections.CollectionChanged += (_, _) => SaveSettings();
        }

        SaveSettings();
    }
}

public partial class Connection : ObservableObject, IConnection
{
    public Connection()
    {
    }

    public Connection(string driverName, string connectionName)
    {
        DriverName = driverName;
        ConnectionName = connectionName;
    }

    [ObservableProperty]
    public partial string? DriverName { get; set; }
    [ObservableProperty]
    public partial string? ConnectionName { get; set; }
}

public partial class FsCockpitConnection : Connection
{
    public FsCockpitConnection()
    {
    }

    public FsCockpitConnection(string driverName, string connectionName) : base(driverName, connectionName)
    {
    }

    [ObservableProperty]
    public partial bool HasHighTensionDetents { get; set; }
}