using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;

namespace DeviceInterfaceManager.Views;

public partial class ProfileCreatorView : UserControl
{
    public ProfileCreatorView()
    {
        InitializeComponent();
    }

    private void InputElementOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Control ctl)
        {
            FlyoutBase.ShowAttachedFlyout(ctl);
        }
    }

    private void ToggleButtonOnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not CommandBarToggleButton commandBarToggleButton)
        {
            return;
        }

        if (commandBarToggleButton.IsChecked is null)
        {
            return;
        }

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: not null } desktop)
        {
            desktop.MainWindow.Topmost = commandBarToggleButton.IsChecked.Value;
        }
    }
}