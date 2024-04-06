namespace DeviceInterfaceManager.Models;

public class DigitFormatting
{    
    public DigitFormatting(int digit)
    {
        Digit = digit;
    }

    public DigitFormatting(int digit, byte? digitCheckedSum, byte? decimalPointCheckedSum)
    {
        Digit = digit;
        IsDigitChecked = CheckSum(digitCheckedSum);
        IsDecimalPointChecked = CheckSum(decimalPointCheckedSum);
    }

    private bool CheckSum(byte? checkedSum)
    {
        return checkedSum != null && (checkedSum & (1 << (Digit - 1))) != 0;
    }

    public int Digit { get; }

    public bool IsDigitChecked { get; set; }
    
    public bool IsDecimalPointChecked { get; set; }
    

    public byte? GetDigitCheckedSum(byte? digitCheckedSum)
    {
        return GetCheckedSum(IsDigitChecked, digitCheckedSum);
    }
    
    public byte? GetDecimalPointCheckedSum(byte? decimalPointCheckedSum)
    {
       return GetCheckedSum(IsDecimalPointChecked, decimalPointCheckedSum);
    }
    
    private byte? GetCheckedSum(bool isChecked, byte? checkedSum)
    {
        if (isChecked)
        {
            return (byte)((checkedSum ?? 0) | (1 << (Digit - 1)));
        }

        return (byte)((checkedSum ?? 0) & ~(1 << (Digit - 1)));
    }
}