using System;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DeviceInterfaceManager.Models.Modifiers;

public partial class Padding : ObservableObject, IModifier
{
    [ObservableProperty]
    public partial bool IsActive { get; set; } = true;

    [ObservableProperty]
    public partial char? Character { get; set; } = Zero;

    public static char[] Characters => [Space, Zero, One];

    private const char Space = ' ';
    private const char Zero = '0';
    private const char One = '1';

    [ObservableProperty]
    public partial int Length { get; set; } = 5;

    [ObservableProperty]
    public partial PaddingDirection Direction { get; set; } = PaddingDirection.Left;

    public static PaddingDirection[] PaddingDirections => Enum.GetValues<PaddingDirection>();

    public enum PaddingDirection
    {
        Left,
        Right
    }

    public void Apply(ref StringBuilder value)
    {
        int fullLength = CountDecimalPoints(value) + Length;

        if (value.Length == fullLength)
        {
            return;
        }
        
        if (value.Length > fullLength)
        {
            value.Length = fullLength;
            return;
        }

        if (Character is null)
        {
            return;
        }

        switch (Direction)
        {
            case PaddingDirection.Left:
                while (value.Length < fullLength)
                {
                    value.Insert(0, Character);
                }
                break;

            case PaddingDirection.Right:
                value.Append(Character.Value, fullLength - value.Length);
                break;
        }
    }

    private static int CountDecimalPoints(StringBuilder value)
    {
        int count = 0;
        for (int i = 0; i < value.Length; i++)
        {
            if (value[i] == '.')
            {
                count++;
            }
        }

        return count;
    }

    public object Clone()
    {
        return MemberwiseClone();
    }
}