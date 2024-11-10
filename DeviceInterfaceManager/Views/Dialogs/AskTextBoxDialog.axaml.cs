using Avalonia.Controls;
using Avalonia.Threading;

namespace DeviceInterfaceManager.Views.Dialogs;

public partial class AskTextBoxDialog : UserControl
{
    public AskTextBoxDialog()
    {
        InitializeComponent();
        TextBox.AttachedToVisualTree += (_, _) => Dispatcher.UIThread.InvokeAsync(() => TextBox.Focus());
    }
}