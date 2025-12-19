using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DeviceInterfaceManager.Models.Modifiers;

public partial class Inserting : ObservableObject, IModifier
{
    [ObservableProperty]
    private bool _isActive = true;

    [ObservableProperty]
    private string? _text = ":";
    
    [ObservableProperty]
    private int _position = 1;

    public void Apply(ref StringBuilder value)
    {
        if (Text is null)
        {
            return;
        }

        value.Insert(Position, Text);
    }

    public object Clone()
    {
        return MemberwiseClone();
    }
}