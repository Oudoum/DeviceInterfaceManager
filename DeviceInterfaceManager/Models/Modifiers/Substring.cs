using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DeviceInterfaceManager.Models.Modifiers;

public partial class Substring : ObservableObject, IModifier
{
    [ObservableProperty]
    public partial bool IsActive { get; set; } = true;

    [ObservableProperty]
    public partial int Start { get; set; }

    [ObservableProperty]
    public partial int End { get; set; } = 7;

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