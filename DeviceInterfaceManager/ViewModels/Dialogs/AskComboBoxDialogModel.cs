using System.Collections.ObjectModel;
using DeviceInterfaceManager.Models.Devices;
using DeviceInterfaceManager.Services.Devices;

namespace DeviceInterfaceManager.ViewModels.Dialogs;

public class AskComboBoxDialogModel : AskTextBoxDialogModel
{
    public ObservableCollection<IDeviceService>? ObservableCollection { get; set; }

    public IDeviceService? SelectedItem { get; set; }
}