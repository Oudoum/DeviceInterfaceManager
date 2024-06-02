using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

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
}