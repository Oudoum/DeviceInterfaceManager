using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.Models;

public interface IOutputCreator
{
    public string? OutputType { get; set; }

    public Component? Output { get; set; }

    public string? DataType { get; set; }
    
    public string? PmdgData { get; set; }
    
    public int? PmdgDataArrayIndex { get; set; }

    public char? Operator { get; set; }
    
    public string? ComparisonValue { get; set; }

    public double? TrueValue { get; set; }

    public double? FalseValue { get; set; }

    public string? Data { get; set; }

    public string? Unit { get; set; }

    public bool IsInverted { get; set; }
    
    public string? NumericFormat { get; set; }

    public bool? IsPadded { get; set; }

    public char? PaddingCharacter { get; set; }

    public byte? DigitCount { get; set; }
    
    public byte? DigitCheckedSum { get; set; }

    public byte? DecimalPointCheckedSum { get; set; }

    public byte? SubstringStart { get; set; }

    public byte? SubstringEnd { get; set; }
}