using DeviceInterfaceManager.ViewModels;
using DeviceInterfaceManager.ViewModels.Dialogs;
using DeviceInterfaceManager.Views;
using DeviceInterfaceManager.Views.Dialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;

namespace DeviceInterfaceManager;

public class ViewLocator : StrongViewLocator
{
    public ViewLocator()
    {
        Register<AskTextBoxDialogModel, AskTextBoxDialog>();
        Register<AskComboBoxDialogModel, AskComboBoxDialog>();
        Register<DeviceViewModel, DeviceView>();
        Register<HomeViewModel, HomeView>();
        Register<InformationViewModel, InformationView>();
        Register<InputTestViewModel, InputTestView>();
        Register<MainWindowViewModel, MainWindow>();
        Register<OutputTestViewModel, OutputTestView>();
        Register<ProfileCreatorViewModel, ProfileCreatorView>();
        Register<SettingsViewModel, SettingsView>();
        Register<InputCreatorViewModel, InputCreatorView>();
        Register<OutputCreatorViewModel, OutputCreatorView>();
    }
}