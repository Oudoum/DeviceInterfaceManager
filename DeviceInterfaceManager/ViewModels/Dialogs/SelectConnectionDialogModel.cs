using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceInterfaceManager.Models;

namespace DeviceInterfaceManager.ViewModels.Dialogs;

public partial class SelectConnectionDialogModel : AskTextBoxDialogModel
{
    public string? DriverName { get; set; }

    public ObservableCollection<IConnection>? Connections { get; set; }
    
    public List<IConnection> FilteredConnections => Connections?.Where(connection => connection.DriverName == DriverName).ToList() ?? [];

    [ObservableProperty]
    private IConnection? _preSelectedConnection = new Connection();

    private IConnection? _selectedConnection;

    public IConnection? SelectedConnection
    {
        get => _selectedConnection;
        set
        {
            _selectedConnection = value;
            if (value is null)
            {
                PreSelectedConnection = GetSelectedConnection();
                return;
            }

            PreSelectedConnection = value;
        }
    }

    [RelayCommand]
    protected void AddMapping()
    {
        if (string.IsNullOrEmpty(DriverName) || PreSelectedConnection?.ConnectionName is null)
        {
            return;
        }

        if (Connections is null || Connections.Any(x => x.ConnectionName == PreSelectedConnection.ConnectionName))
        {
            return;
        }

        Connections ??= [];

        IConnection? newConnection = DriverName switch
        {
            ProfileCreatorModel.FdsEnet => new Connection(DriverName, PreSelectedConnection.ConnectionName),
            ProfileCreatorModel.FsCockpit => new FsCockpitConnection(DriverName, PreSelectedConnection.ConnectionName),
            _ => null
        };

        if (newConnection is null)
        {
            return;
        }

        Connections.Add(newConnection);
        OnPropertyChanged(nameof(FilteredConnections));
        PreSelectedConnection = GetSelectedConnection();
    }

    private IConnection? GetSelectedConnection()
    {
        IConnection? newConnection = DriverName switch
        {
            ProfileCreatorModel.FdsEnet => new Connection(),
            ProfileCreatorModel.FsCockpit => new FsCockpitConnection(),
            _ => null
        };

        return newConnection;
    }

    [RelayCommand]
    private void RemoveMapping()
    {
        if (SelectedConnection is null)
        {
            return;
        }

        Connections?.Remove(SelectedConnection);
        OnPropertyChanged(nameof(FilteredConnections));
    }
}