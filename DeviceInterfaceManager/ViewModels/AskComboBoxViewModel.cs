using System.Collections.ObjectModel;
using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.ViewModels;

public class AskComboBoxViewModel : AskTextBoxViewModel
{
    public ObservableCollection<IInputOutputDevice>? ObservableCollection { get; set; }
    
    public IInputOutputDevice? SelectedItem { get; set; }
}