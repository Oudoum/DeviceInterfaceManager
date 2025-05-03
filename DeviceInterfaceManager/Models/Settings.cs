using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DeviceInterfaceManager.Models;

public partial class Settings : ObservableObject
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
    private bool _fsCockpit;

    [ObservableProperty]
    private ObservableCollection<IConnection>? _connections = [];

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
    private string? _driverName;

    [ObservableProperty]
    private string? _connectionName;
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
    private bool _hasHighTensionDetents;
}