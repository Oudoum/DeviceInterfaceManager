using DeviceInterfaceManager.Models.Modifiers;

namespace DeviceInterfaceManager.Models;

public interface IOutputCreator : IDescription
{
    public string? OutputType { get; set; }

    public int[]? Outputs { get; set; }

    public string? DataType { get; set; }

    public string? Data { get; set; }

    public string? Unit { get; set; }

    public IModifier[]? Modifiers { get; set; }

    public Display? Display { get; set; }
}