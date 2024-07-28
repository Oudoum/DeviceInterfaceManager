using System;

namespace DeviceInterfaceManager.Models;

public interface IPrecondition : IActive
{
    public Guid ReferenceId { get; set; }
    
    public char? Operator { get; set; }
    
    public string? ComparisonValue { get; set; }
    
    public bool IsOrOperator { get; set; }
}