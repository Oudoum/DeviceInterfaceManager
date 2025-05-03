using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using DeviceInterfaceManager.Models;
using DeviceInterfaceManager.Services.Devices.Fds;

namespace DeviceInterfaceManager.ViewModels.Dialogs;

public partial class SelectInternetProtocolDialogModel : SelectConnectionDialogModel
{
    [RelayCommand(CanExecute = nameof(CanSearchForDevices))]
    private async Task SearchForDevicesAsync()
    {
        var connections = await InterfaceItEthernetService.ReceiveControllerDiscoveryDataAsync();
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