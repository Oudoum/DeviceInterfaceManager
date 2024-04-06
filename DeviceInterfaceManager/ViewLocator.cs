using DeviceInterfaceManager.ViewModels;
using DeviceInterfaceManager.Views;
using HanumanInstitute.MvvmDialogs.Avalonia;

namespace DeviceInterfaceManager;

public class ViewLocator : StrongViewLocator
{
    public ViewLocator()
    {
        Register<AskTextBoxViewModel, AskTextBoxView>();
        Register<AskComboBoxViewModel, AskComboBoxView>();
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