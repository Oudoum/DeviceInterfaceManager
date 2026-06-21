using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceInterfaceManager.Models;
using DeviceInterfaceManager.Models.Devices;
using DeviceInterfaceManager.Models.FlightSim.MSFS.PMDG;
using DeviceInterfaceManager.Models.Modifiers;
using DeviceInterfaceManager.Services.Devices;
using HanumanInstitute.MvvmDialogs;

namespace DeviceInterfaceManager.ViewModels;

public partial class InputCreatorViewModel : BaseCreatorViewModel, IInputCreator, IViewClosed
{
    private readonly IInputCreator _inputCreator;

    public InputCreatorViewModel(IDeviceService deviceService, IInputCreator inputCreator, IReadOnlyCollection<OutputCreator> outputCreators, IEnumerable<IPrecondition>? preconditions)
        : base(deviceService, outputCreators, preconditions)
    {
        _inputCreator = inputCreator;
        Description = _inputCreator.Description;
        InputType = inputCreator.InputType;
        Components = GetComponents(InputType);
        Component = Components?.FirstOrDefault(x => x?.Position == inputCreator.Input);
        EventType = inputCreator.EventType;
        Event = inputCreator.Event;
        SearchPmdgEvent = Event;
        DataPress = inputCreator.DataPress;
        DataPress2 = inputCreator.DataPress2;
        DataRelease = inputCreator.DataRelease;
        DataRelease2 = inputCreator.DataRelease2;
        Interpolation = inputCreator.Interpolation;

        DeviceService.SwitchPositionChanged += SwitchPositionChanged;
        DeviceService.AnalogValueChanged += AnalogValueChanged;
    }

    public void OnClosed()
    {
        DeviceService.SwitchPositionChanged -= SwitchPositionChanged;
        DeviceService.AnalogValueChanged -= AnalogValueChanged;
    }

    public bool GetPosition { get; set; }

    private void SwitchPositionChanged(object? sender, SwitchPositionChangedEventArgs e)
    {
        if (InputType == ProfileCreatorModel.Switch && GetPosition && e.IsPressed)
        {
            Component = Components?.FirstOrDefault(i => i?.Position == e.Position);
        }
    }

    private void AnalogValueChanged(object? sender, AnalogValueChangedEventArgs e)
    {
        if (InputType == ProfileCreatorModel.Analog && GetPosition)
        {
            Component = Components?.FirstOrDefault(i => i?.Position == e.Position);
        }
    }

    public override Precondition[]? Copy()
    {
        _inputCreator.Description = GetDescription();
        _inputCreator.InputType = InputType;
        _inputCreator.Input = Component?.Position;
        _inputCreator.EventType = EventType;
        _inputCreator.Event = Event;
        _inputCreator.DataPress = DataPress;
        _inputCreator.DataPress2 = DataPress2;
        _inputCreator.DataRelease = DataRelease;
        _inputCreator.DataRelease2 = DataRelease2;
        _inputCreator.Interpolation = Interpolation;
        return base.Copy();
    }

    private string? GetDescription()
    {
        if (!string.IsNullOrEmpty(Description))
        {
            return Description;
        }

        return Event ?? null;
    }

    public string? Description { get; set; }

    [ObservableProperty]
    public partial bool IsAnalog { get; set; }

    [ObservableProperty]
    public partial string? InputType { get; set; }

    partial void OnInputTypeChanged(string? value)
    {
        switch (value)
        {
            case ProfileCreatorModel.Switch:
                Components = GetComponents(value);
                DestroyInterpolation();
                IsAnalog = false;
                break;

            case ProfileCreatorModel.Analog:
                Components = GetComponents(value);
                IsAnalog = true;
                DataPress = null;
                DataPress2 = null;
                DataRelease = null;
                DataRelease2 = null;
                break;
        }
    }

    private IEnumerable<Component?>? GetComponents(string? value)
    {
        return value switch
        {
            ProfileCreatorModel.Switch => DeviceService.Inputs?.Switch.Components,
            ProfileCreatorModel.Analog => DeviceService.Inputs?.Analog.Components,
            _ => Components
        };
    }

    public static string[] InputTypes => [ProfileCreatorModel.Switch, ProfileCreatorModel.Analog];

    [ObservableProperty]
    public partial IEnumerable<Component?>? Components { get; set; }

    [ObservableProperty]
    public partial Component? Component { get; set; }

    public int? Input { get; set; }

    private string? _eventType;

    public string? EventType
    {
        get => _eventType;
        set
        {
            switch (value)
            {
                case ProfileCreatorModel.MsfsSimConnect:
                    IsMsfsSimConnect = true;
                    break;

                case ProfileCreatorModel.KEvent:
                    IsKEvent = true;
                    break;

                case ProfileCreatorModel.Rpn:
                    IsRpn = true;
                    break;

                case ProfileCreatorModel.Pmdg737:
                    IsPmdg737 = true;
                    break;

                case ProfileCreatorModel.Pmdg777:
                    IsPmdg777 = true;
                    break;
            }

            _eventType = value;
        }
    }

    [ObservableProperty]
    public partial bool IsMsfsSimConnect { get; set; }

    partial void OnIsMsfsSimConnectChanged(bool value)
    {
        if (!value)
        {
            return;
        }

        EventType = ProfileCreatorModel.MsfsSimConnect;
        IsKEvent = false;
        IsRpn = false;
        IsPmdg737 = false;
        IsPmdg777 = false;
        ResetEventType();
        DataPress2 = null;
        DataRelease2 = null;
    }

    [ObservableProperty]
    public partial bool IsKEvent { get; set; }

    partial void OnIsKEventChanged(bool value)
    {
        if (!value)
        {
            return;
        }

        EventType = ProfileCreatorModel.KEvent;
        IsMsfsSimConnect = false;
        IsRpn = false;
        IsPmdg737 = false;
        IsPmdg777 = false;
        ResetEventType();
    }

    [ObservableProperty]
    public partial bool IsRpn { get; set; }

    partial void OnIsRpnChanged(bool value)
    {
        if (!value)
        {
            return;
        }

        EventType = ProfileCreatorModel.Rpn;
        IsMsfsSimConnect = false;
        IsKEvent = false;
        IsPmdg737 = false;
        IsPmdg777 = false;
        ResetEventType();
        DataPress2 = null;
        DataRelease2 = null;
    }

    [ObservableProperty]
    public partial bool IsPmdg { get; set; }

    [ObservableProperty]
    public partial bool IsPmdg737 { get; set; }

    partial void OnIsPmdg737Changed(bool value)
    {
        if (!value)
        {
            return;
        }

        EventType = ProfileCreatorModel.Pmdg737;
        OnPmdgChanged();
        PmdgEventEnumerable = Enum.GetNames(typeof(B737.Event));
    }

    [ObservableProperty]
    public partial bool IsPmdg777 { get; set; }

    partial void OnIsPmdg777Changed(bool value)
    {
        if (!value)
        {
            return;
        }

        EventType = ProfileCreatorModel.Pmdg777;
        OnPmdgChanged();
        PmdgEventEnumerable = Enum.GetNames(typeof(B777.Event));
    }

    private void OnPmdgChanged()
    {
        IsMsfsSimConnect = false;
        IsKEvent = false;
        IsRpn = false;
        IsPmdg = true;
        Event = null;
        ClearPmdgMousePress();
        ClearPmdgMouseRelease();
        SearchPmdgEvent = null;
        DataPress2 = null;
        DataRelease2 = null;
    }

    private void ResetEventType()
    {
        IsPmdg = false;
        Event = null;
        ClearPmdgMousePress();
        ClearPmdgMouseRelease();
    }

    [ObservableProperty]
    public partial string? Event { get; set; }

    partial void OnEventChanged(string? value)
    {
        if (value == string.Empty)
        {
            Event = null;
        }
    }

    [ObservableProperty]
    public partial long? DataPress { get; set; }

    [ObservableProperty]
    public partial long? DataPress2 { get; set; }

    [ObservableProperty]
    public partial long? DataRelease { get; set; }

    [ObservableProperty]
    public partial long? DataRelease2 { get; set; }

    [ObservableProperty]
    public partial string? SearchPmdgEvent { get; set; }

    partial void OnSearchPmdgEventChanged(string? value)
    {
        if (IsPmdg737 && Enum.TryParse(value, true, out B737.Event _))
        {
            Event = value;
            return;
        }

        if (IsPmdg777 && Enum.TryParse(value, true, out B777.Event _))
        {
            Event = value;
        }
    }

    [ObservableProperty]
    public partial IEnumerable<string?>? PmdgEventEnumerable { get; set; }

    public static Mouse[] PmdgMouseFlags =>
    [
        Mouse.LeftSingle,
        Mouse.LeftRelease,
        Mouse.RightSingle,
        Mouse.RightRelease,
        Mouse.WheelDown,
        Mouse.WheelUp
    ];

    [RelayCommand]
    private void ClearPmdgMousePress() => DataPress = null;
    
    [RelayCommand]
    private void ClearPmdgMouseRelease() => DataRelease = null;

    [ObservableProperty]
    public partial Interpolation? Interpolation { get; set; }

    [RelayCommand]
    private void CreateInterpolation() => Interpolation ??= new Interpolation();

    [RelayCommand]
    private void DestroyInterpolation() => Interpolation = null;
}