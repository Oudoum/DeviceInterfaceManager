using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DeviceInterfaceManager.Models.Modifiers;

public class Blinking : ObservableObject, IModifier
{
    public bool IsActive { get; set; }

    public void Apply(ref StringBuilder value)
    {

    }

    public object Clone()
    {
        return MemberwiseClone();
    }
}