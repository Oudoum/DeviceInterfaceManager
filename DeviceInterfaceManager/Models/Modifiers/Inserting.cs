using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DeviceInterfaceManager.Models.Modifiers;

public partial class Inserting : ObservableObject, IModifier
{
    [ObservableProperty]
    private bool _isActive = true;

    [ObservableProperty]
    private char? _character = ':';
    
    [ObservableProperty]
    private int _position = 1;

    public void Apply(ref StringBuilder value)
    {
        if (Character is null)
        {
            return;
        }

        value.Insert(Position, Character.Value);
    }

    public object Clone()
    {
        return MemberwiseClone();
    }
}