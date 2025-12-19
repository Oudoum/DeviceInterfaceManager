using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DeviceInterfaceManager.Models;

public partial class Display : ObservableObject, ICloneable
{
    public Display()
    {
        DigitCount = 3;
    }
    
    [ObservableProperty]
    private bool _isLeftPadded = true;

    public static Dictionary<string, char> PaddingCharacters => new() { ["Zero"] = '0', ["Space"] = ' ' };

    [ObservableProperty]
    private char _paddingCharacter = PaddingCharacters["Zero"];

    public static int[] DigitCounts => [1, 2, 3, 4, 5, 6, 7, 8];

    [JsonIgnore]
    public int DigitCount
    {
        get => Digits.Count;
        set
        {
            if (Digits.Count > value)
            {
                for (int i = Digits.Count - 1; i >= value; i--)
                {
                    Digits[i].IsDigitChecked = false;
                    Digits[i].IsDecimalPointChecked = false;
                    Digits.RemoveAt(i);
                }

                return;
            }

            for (int i = Digits.Count; i < value; i++)
            {
                Digits.Add(new DigitFormatting(i + 1));
            }
        }
    }

    public ObservableCollection<DigitFormatting> Digits { get; set; } = [new(1), new(2), new(3)];

    public void SetDisplayValue(ref StringBuilder value)
    {
        _ = value.Replace(".", string.Empty);
        if (value.Length > DigitCount)
        {
            int digitCount = DigitCount;
            switch (value[digitCount] - '0')
            {
                case > 5:
                {
                    value.Length = digitCount;
                    int carry = 1;
                    for (int i = digitCount - 1; i >= 0; i--)
                    {
                        int digit = value[i] - '0' + carry;
                        carry = digit / 10;
                        value[i] = (char)(digit % 10 + '0');
                    }

                    if (carry > 0)
                    {
                        _ = value.Insert(0, carry);
                    }

                    break;
                }

                case <= 5:
                    value.Length = digitCount;
                    break;
            }
        }

        if (value.Length > DigitCount)
        {
            value.Length = DigitCount;
        }

        switch (IsLeftPadded)
        {
            case true:
                while (value.Length < DigitCount)
                {
                    value.Insert(0, PaddingCharacter);
                }

                break;

            case false:
                value.Append(PaddingCharacter, DigitCount - value.Length);
                break;
        }

        FormatDigits(ref value);
    }

    private void FormatDigits(ref StringBuilder value)
    {
        int decimalPointCount = 0;
        foreach (DigitFormatting digit in Digits)
        {
            int position = digit.Digit - 1 + decimalPointCount;
            switch (digit.IsDigitChecked)
            {
                case false:
                    value[position] = ' ';
                    break;

                case true when value.Length <= position:
                    _ = value.Append(PaddingCharacter);
                    break;
            }

            if (!digit.IsDecimalPointChecked)
            {
                continue;
            }

            _ = value.Insert(digit.Digit + decimalPointCount, '.');
            decimalPointCount++;
        }
    }

    public object Clone()
    {
        throw new NotImplementedException();
    }
}