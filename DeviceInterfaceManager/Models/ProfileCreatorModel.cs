using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DeviceInterfaceManager.Models.Devices;
using DeviceInterfaceManager.Models.FlightSim.MSFS.PMDG.SDK;
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

    //Data-/EventTypes
    public const string Pmdg737 = "PMDG737";
    public const string Pmdg747 = "PMDG747";
    public const string Pmdg777 = "PMDG777";
    public const string KEvent = "K:Event";
    public const string Dim = "DIMVar";
    public const string MsfsSimConnect = "MSFS/SimConnect/LVar";
    public const string Rpn = "RPN/H-Events";
    public const string XPlane = "XPlane";

    //Inputs
    public const string Switch = "Switch";
    public const string AnalogIn = "AnalogIn";

    //Outputs
    public const string Led = "LED";
    public const string SevenSegment = "7 Segment";
    public const string Dataline = "Dataline";
    public const string AnalogOut = "AnalogOut";

    [ObservableProperty]
    private string? _profileName;

    [ObservableProperty]
    private string? _description;

	[ObservableProperty]
    private string? _deviceName;

    [ObservableProperty]
    private ObservableCollection<InputCreator> _inputCreators = [];

    [ObservableProperty]
    private ObservableCollection<OutputCreator> _outputCreators = [];
}

public partial class InputCreator : ObservableObject, IInputCreator, IActive
{
    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    private bool _isActive;

    [ObservableProperty]
    private string? _description;
    
    partial void OnDescriptionChanged(string? value)
    {
        if (value == string.Empty)
        {
            Description = null;
        }
    }

    [ObservableProperty]
    private string? _inputType;

    [ObservableProperty]
    private Component? _input;

    [ObservableProperty]
    private string? _eventType;

    [ObservableProperty]
    private string? _event;

    [ObservableProperty]
    private long? _dataPress;

    [ObservableProperty]
    private long? _dataPress2;

    [ObservableProperty]
    private long? _dataRelease;
    
    [ObservableProperty]
    private long? _dataRelease2;

    [ObservableProperty]
    private int? _pmdgEvent;

    [ObservableProperty]
    private Mouse? _pmdgMousePress;

    [ObservableProperty]
    private Mouse? _pmdgMouseRelease;

    [ObservableProperty]
    private bool _onRelease;

    [ObservableProperty]
    private Precondition[]? _preconditions;

    public InputCreator Clone()
    {
        InputCreator? clone = MemberwiseClone() as InputCreator;
        clone!.Id = Guid.NewGuid();
        return clone;
    }
}

public partial class OutputCreator : ObservableObject, IOutputCreator, IActive
{
    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    private bool _isActive;

    [ObservableProperty]
    private string? _description;
    
    partial void OnDescriptionChanged(string? value)
    {
        if (value == string.Empty)
        {
            Description = null;
        }
    }

    [ObservableProperty]
    private string? _outputType;

    [ObservableProperty]
    private int[]? _outputs;

    [ObservableProperty]
    private string? _dataType;

    [ObservableProperty]
    private string? _data;

    [ObservableProperty]
    private string? _unit;

    [ObservableProperty]
    private string? _pmdgData;

    [ObservableProperty]
    private int? _pmdgDataArrayIndex;

    [ObservableProperty]
    private IModifier[]? _modifiers;

    [ObservableProperty]
    private bool? _isPadded;

    [ObservableProperty]
    private char? _paddingCharacter;

    [ObservableProperty]
    private byte? _digitCount;

    [ObservableProperty]
    private byte? _digitCheckedSum;

    [ObservableProperty]
    private byte? _decimalPointCheckedSum;

    [ObservableProperty]
    [property: JsonIgnore]
    private string? _flightSimValue;

    [ObservableProperty]
    [property: JsonIgnore]
    private string? _outputValue;

    [ObservableProperty]
    private Precondition[]? _preconditions;

    public OutputCreator Clone()
    {
        OutputCreator clone = MemberwiseClone() as OutputCreator ?? new OutputCreator();
        clone.Id = Guid.NewGuid();
        clone.OutputValue = null;
        clone.FlightSimValue = null;
        return clone;
    }
}

public partial class Precondition : ObservableObject, IPrecondition
{
    [ObservableProperty]
    private bool _isActive;

    [ObservableProperty]
    private Guid _referenceId;

    [ObservableProperty]
    private char? _operator;

    [ObservableProperty]
    private string? _comparisonValue;

    [ObservableProperty]
    private bool _isOrOperator;
}