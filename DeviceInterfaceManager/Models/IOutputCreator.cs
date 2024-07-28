using DeviceInterfaceManager.Models.Modifiers;

namespace DeviceInterfaceManager.Models;

public interface IOutputCreator
{
    public string? Description { get; set; }
    
    public string? OutputType { get; set; }

    public int[]? Outputs { get; set; }

    public string? DataType { get; set; }

    public string? Data { get; set; }

    public string? Unit { get; set; }
    
    public string? PmdgData { get; set; }
    
    public int? PmdgDataArrayIndex { get; set; }
    
    public IModifier[]? Modifiers { get; set; } 

    public bool? IsPadded { get; set; }

    public char? PaddingCharacter { get; set; }

    public byte? DigitCount { get; set; }
    
    public byte? DigitCheckedSum { get; set; }

    public byte? DecimalPointCheckedSum { get; set; }
}