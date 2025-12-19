using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using DeviceInterfaceManager.Models;
using DeviceInterfaceManager.Services.Devices.Fds;
using Microsoft.Extensions.Logging;

namespace DeviceInterfaceManager.ViewModels.Dialogs;

public partial class SelectInternetProtocolDialogModel : SelectConnectionDialogModel
{
    private readonly ILogger _logger;

    public SelectInternetProtocolDialogModel(ILogger<SelectInternetProtocolDialogModel> logger)
    {
        _logger = logger;
    }

    [RelayCommand(CanExecute = nameof(CanSearchForDevices))]
    private async Task SearchForDevicesAsync()
    {
        var connections = await InterfaceItEthernetService.ReceiveControllerDiscoveryDataAsync(_logger);
        foreach (string connection in connections.Where(connection => !string.IsNullOrEmpty(connection)))
        {
            if (PreSelectedConnection is null)
            {
                continue;
            }

            PreSelectedConnection.ConnectionName = connection;
            AddMapping();
        }
    }

    private bool CanSearchForDevices()
    {
        return DriverName == ProfileCreatorModel.FdsEnet;
    }
}