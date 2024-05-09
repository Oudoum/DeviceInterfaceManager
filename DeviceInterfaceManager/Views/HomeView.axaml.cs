using Avalonia.Controls;
using Avalonia.Input;
using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.Views;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();
        
        MyButton.AddHandler(DragDrop.DropEvent, Drop);
    }
    
    private void Drop(object? sender, DragEventArgs e)
    {
        object? data = e.Data.Get(nameof(IInputOutputDevice));

        if (data is IInputOutputDevice inputOutputDevice)
        {
            MyButton.Content = inputOutputDevice;
        }
    }
}