using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DeviceInterfaceManager.Models.Modifiers;

public partial class Interpolation : ObservableObject, IModifier
{
    [ObservableProperty]
    private bool _isActive = true;
    
    public void Apply(ref StringBuilder value)
    {
        
    }
    
    public object Clone()
    {
        return MemberwiseClone();
    }
}