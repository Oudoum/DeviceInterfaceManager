using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DeviceInterfaceManager.Models.Modifiers;

#pragma warning disable CS0657 // Not a valid attribute location for this declaration

namespace DeviceInterfaceManager.Models;

public partial class ProfileCreatorModel : ObservableObject
{
    //Drivers
    public const string FdsUsb = "FDS USB";
    public const string FdsEnet = "FDS E-Series";
    public const string CPflightUsb = "CPflight USB";
    public const string CPflightEnet = "CPflight ENET";
    public const string Hid = "HID";
    public const string Arduino = "Arduino";
    public const string Sioc = "SIOC";
    public const string FsCockpit = "FSCockpit";

    //Data-/EventTypes
    public const string Pmdg737 = "PMDG737";
    public const string Pmdg747 = "PMDG747";
    public const string Pmdg777 = "PMDG777";
    public const string KEvent = "K:Event";
    public const string Dim = "DIMVar";
    public const string MsfsSimConnect = "MSFS/SimConnect/LVar";
    public const string Rpn = "RPN/H-Events";
    public const string XPlane = "XPlane";

    //Inputs & Outputs
    public const string Analog = "Analog";

    //Inputs
    public const string Switch = "Switch";

    //Outputs
    public const string Led = "LED";
    public const string Dataline = "Dataline";
    public const string SevenSegment = "7 Segment";

    [ObservableProperty]
    public partial string? Description { get; set; }

    [ObservableProperty]
    public partial string? DeviceName { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<InputCreator> InputCreators { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<OutputCreator> OutputCreators { get; set; } = [];
}

public partial class InputCreator : ObservableObject, IInputCreator, IActive, ICloneable
{
    [ObservableProperty]
    public partial Guid Id { get; set; } = Guid.NewGuid();

    [ObservableProperty]
    public partial bool IsActive { get; set; } = true;

    [ObservableProperty]
    public partial string? Description { get; set; }

    partial void OnDescriptionChanged(string? value)
    {
        if (value == string.Empty)
        {
            Description = null;
        }
    }

    [ObservableProperty]
    public partial string? InputType { get; set; }

    [ObservableProperty]
    public partial int? Input { get; set; }

    [ObservableProperty]
    public partial string? EventType { get; set; }

    [ObservableProperty]
    public partial string? Event { get; set; }

    [ObservableProperty]
    public partial long? DataPress { get; set; }

    [ObservableProperty]
    public partial long? DataPress2 { get; set; }

    [ObservableProperty]
    public partial long? DataRelease { get; set; }

    [ObservableProperty]
    public partial long? DataRelease2 { get; set; }

    [ObservableProperty]
    public partial Interpolation? Interpolation { get; set; }

    [ObservableProperty]
    public partial Precondition[]? Preconditions { get; set; }

    public object Clone()
    {
        InputCreator clone = MemberwiseClone() as InputCreator ?? new InputCreator();
        clone.Id = Guid.NewGuid();
        clone.Description = null;

        if (Interpolation is null)
        {
            return clone;
        }

        clone.Interpolation = (Interpolation)Interpolation.Clone();
        return clone;
    }
}

public partial class OutputCreator : ObservableObject, IOutputCreator, IActive, ICloneable
{
    [ObservableProperty]
    public partial Guid Id { get; set; } = Guid.NewGuid();

    [ObservableProperty]
    public partial bool IsActive { get; set; } = true;

    [ObservableProperty]
    public partial string? Description { get; set; }

    partial void OnDescriptionChanged(string? value)
    {
        if (value == string.Empty)
        {
            Description = null;
        }
    }

    [ObservableProperty]
    public partial string? OutputType { get; set; }

    [ObservableProperty]
    public partial int[]? Outputs { get; set; }

    [ObservableProperty]
    public partial string? DataType { get; set; }

    [ObservableProperty]
    public partial string? Data { get; set; }

    [ObservableProperty]
    public partial string? Unit { get; set; }

    [ObservableProperty]
    public partial IModifier[]? Modifiers { get; set; }

    [ObservableProperty]
    public partial Display? Display { get; set; }

    [ObservableProperty]
    [JsonIgnore]
    public partial string? FlightSimValue { get; set; }

    [ObservableProperty]
    [JsonIgnore]
    public partial string? OutputValue { get; set; }

    [ObservableProperty]
    public partial Precondition[]? Preconditions { get; set; }

    public object Clone()
    {
        OutputCreator clone = MemberwiseClone() as OutputCreator ?? new OutputCreator();
        clone.Id = Guid.NewGuid();
        clone.Description = null;
        clone.OutputValue = null;
        clone.FlightSimValue = null;

        if (Modifiers is null)
        {
            return clone;
        }

        clone.Modifiers = new IModifier[Modifiers.Length];
        for (int i = 0; i < Modifiers.Length; i++)
        {
            clone.Modifiers[i] = (IModifier)Modifiers[i].Clone();
        }

        return clone;
    }
}

public partial class Precondition : ObservableObject, IPrecondition
{
    [ObservableProperty]
    public partial bool IsActive { get; set; }

    [ObservableProperty]
    public partial Guid ReferenceId { get; set; }

    [ObservableProperty]
    public partial char? Operator { get; set; }

    [ObservableProperty]
    public partial string? ComparisonValue { get; set; }

    [ObservableProperty]
    public partial bool IsOrOperator { get; set; }

    [ObservableProperty]
    public partial bool UseOutputValue { get; set; }
}