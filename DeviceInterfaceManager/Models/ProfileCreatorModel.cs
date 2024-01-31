using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DeviceInterfaceManager.Models.SimConnect.MSFS.PMDG.SDK;

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

    public static string[] Drivers => [FdsUsb, FdsEnet, CPflightUsb, CPflightEnet, Hid, Arduino, Sioc];

    //Data-/EventTypes
    public const string Pmdg737 = "PMDG737";
    public const string Pmdg747 = "PMDG747";
    public const string Pmdg777 = "PMDG777";
    public const string KEvent = "K:Event";
    public const string MsfsSimConnect = "MSFS/SimConnect/LVar";
    public const string Rpn = "RPN/H-Events";
    public const string XPlane = "XPlane";

    //Inputs
    public const string Switch = "Switch";

    //Outputs
    public const string Led = "LED";
    public const string SevenSegment = "7 Segment";
    public const string Dataline = "Dataline";

    public string? ProfileName { get; set; }

    public string? Driver { get; set; }

    public string? DeviceName { get; set; }

    [ObservableProperty]
    private ObservableCollection<InputCreator> _inputCreator = [];

    [ObservableProperty]
    private ObservableCollection<OutputCreator> _outputCreator = [];
}

public partial class InputCreator : ObservableObject
{
    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    private bool _isActive;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private string? _inputType;

    [ObservableProperty]
    private KeyValuePair<string, string>? _input;

    [ObservableProperty]
    private string? _eventType;

    [ObservableProperty]
    private B737.Event? _pmdgEvent;

    [ObservableProperty]
    private string? _event;

    [ObservableProperty]
    private bool _onRelease;

    [ObservableProperty]
    private KeyValuePair<string, uint>? _pmdgMousePress;

    [ObservableProperty]
    private KeyValuePair<string, uint>? _pmdgMouseRelease;

    [ObservableProperty]
    private uint? _dataPress;

    [ObservableProperty]
    private uint? _dataRelease;

    [ObservableProperty]
    private Precondition[] _preconditions = Array.Empty<Precondition>();

    public InputCreator Clone()
    {
        InputCreator? clone = MemberwiseClone() as InputCreator;
        clone!.Id = Guid.NewGuid();
        return clone;
    }
}

public partial class OutputCreator : ObservableObject
{
    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    private bool _isActive;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private string? _outputType;

    [ObservableProperty]
    private KeyValuePair<string, string>? _output;

    [ObservableProperty]
    private string? _dataType;

    [ObservableProperty]
    private string? _pmdgData;

    [ObservableProperty]
    private string? _data;

    [ObservableProperty]
    private string? _unit;

    [ObservableProperty]
    private int? _pmdgDataArrayIndex;

    [ObservableProperty]
    private char? _operator;

    [ObservableProperty]
    private string? _comparisonValue;

    [ObservableProperty]
    private double? _trueValue;

    [ObservableProperty]
    private double? _falseValue;

    [ObservableProperty]
    private bool _isInverted;

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
    private byte? _substringStart;

    [ObservableProperty]
    private byte? _substringEnd;

    [ObservableProperty]
    [property: JsonIgnore]
    private object? _flightSimValue;

    [ObservableProperty]
    [property: JsonIgnore]
    private object? _outputValue;

    [ObservableProperty]
    private Precondition[] _preconditions = Array.Empty<Precondition>();

    public OutputCreator Clone()
    {
        OutputCreator? clone = MemberwiseClone() as OutputCreator;
        clone!.Id = Guid.NewGuid();
        return clone;
    }
}

public partial class Precondition : ObservableObject
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