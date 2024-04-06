using System;

namespace DeviceInterfaceManager.Models;

public interface IPrecondition
{
    public bool IsActive { get; set; }
    
    public Guid ReferenceId { get; set; }
    
    public char? Operator { get; set; }
    
    public string? ComparisonValue { get; set; }
    
    public bool IsOrOperator { get; set; }
}