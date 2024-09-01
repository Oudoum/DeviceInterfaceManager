using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DeviceInterfaceManager.Models.Modifiers;

public partial class Substring : ObservableObject, IModifier
{
    [ObservableProperty]
    private bool _isActive = true;

    [ObservableProperty]
    private int _start;

    [ObservableProperty]
    private int _end = 7;

    public void Apply(ref StringBuilder value)
    {
        if (Start >= value.Length || Start > End)
        {
           value.Clear();
           return;
        }
        
        value.Remove(0, Start);
        value.Length = End - Start + 1;
    }

    public object Clone()
    {
        return MemberwiseClone();
    }
}