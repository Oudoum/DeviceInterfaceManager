using System;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DeviceInterfaceManager.Models.Modifiers;

public partial class Padding : ObservableObject, IModifier
{
    [ObservableProperty]
    private bool _isActive = true;

    [ObservableProperty]
    private char? _character = Zero;

    public static char[] Characters => [Space, Zero, One];

    private const char Space = ' ';
    private const char Zero = '0';
    private const char One = '1';

    [ObservableProperty]
    private int _length = 5;
    
    [ObservableProperty]
    private PaddingDirection _direction = PaddingDirection.Left;

    public static PaddingDirection[] PaddingDirections => Enum.GetValues<PaddingDirection>();

    public enum PaddingDirection
    {
        Left,
        Right
    }

    public void Apply(ref StringBuilder value)
    {
        if (value.Length > Length)
        {
            value.Length = Length;
            return;
        }

        if (Character is null)
        {
            return;
        }

        switch (Direction)
        {
            case PaddingDirection.Left:
                for (int i = value.Length; i < Length; i++)
                {
                    value.Insert(0, Character);
                }
                break;

            case PaddingDirection.Right:
                value.Append(Character.Value, Length - value.Length);
                break;
        }
    }
    
    public object Clone()
    {
        return MemberwiseClone();
    }
}