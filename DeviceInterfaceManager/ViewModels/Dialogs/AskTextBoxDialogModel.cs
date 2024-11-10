using CommunityToolkit.Mvvm.ComponentModel;

namespace DeviceInterfaceManager.ViewModels.Dialogs;

public class AskTextBoxDialogModel : ObservableObject
{
    public string? Title { get; set; }

    public string? Text { get; set; }
}