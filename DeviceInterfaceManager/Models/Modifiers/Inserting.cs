using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DeviceInterfaceManager.Models.Modifiers;

public partial class Inserting : ObservableObject, IModifier
{
    [ObservableProperty]
    public partial bool IsActive { get; set; } = true;

    [ObservableProperty]
    public partial string? Text { get; set; } = ":";

    [ObservableProperty]
    public partial int Position { get; set; } = 1;

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