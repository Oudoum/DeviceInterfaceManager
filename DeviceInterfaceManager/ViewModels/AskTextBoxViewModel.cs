using CommunityToolkit.Mvvm.ComponentModel;

namespace DeviceInterfaceManager.ViewModels;

public class AskTextBoxViewModel : ObservableObject
{
    public string? Title { get; set; }

    public string? Text { get; set; }
}