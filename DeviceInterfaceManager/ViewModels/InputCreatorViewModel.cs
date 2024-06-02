using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceInterfaceManager.Models;
using DeviceInterfaceManager.Models.Devices;
using DeviceInterfaceManager.Models.FlightSim.MSFS.PMDG.SDK;
using HanumanInstitute.MvvmDialogs;

namespace DeviceInterfaceManager.ViewModels;

public partial class InputCreatorViewModel : BaseCreatorViewModel, IInputCreator, IViewClosed
{
    private readonly IInputCreator _inputCreator;

    public InputCreatorViewModel(IInputOutputDevice inputOutputDevice, IInputCreator inputCreator, IReadOnlyCollection<OutputCreator> outputCreators, IEnumerable<IPrecondition>? preconditions)
        : base(inputOutputDevice, outputCreators, preconditions)
    {
        _inputCreator = inputCreator;
        InputType = inputCreator.InputType;
        Components = GetComponents(InputType);
        Input = Components.FirstOrDefault(x => x?.Position == inputCreator.Input?.Position);
        EventType = inputCreator.EventType;
        PmdgEvent = inputCreator.PmdgEvent;
        PmdgMousePress = inputCreator.PmdgMousePress;
        PmdgMouseRelease = inputCreator.PmdgMouseRelease;
        Event = inputCreator.Event;
        OnRelease = inputCreator.OnRelease;
        DataPress = inputCreator.DataPress;
        DataPress2 = inputCreator.DataPress2;
        DataRelease = inputCreator.DataRelease;
        DataRelease2 = inputCreator.DataRelease2;

        if (PmdgEvent is not null)
        {
            SearchPmdgEvent = Enum.GetName(PmdgEvent.Value);
        }

        InputOutputDevice.InputChanged += InputOutputDeviceOnInputChanged;
    }

    #if DEBUG
    public InputCreatorViewModel()
    {
        _inputCreator =
            new InputCreator
            {
                IsActive = true,
                Preconditions = [new Precondition()],
                Description = "Description",
                InputType = ProfileCreatorModel.Switch,
                Input = new Component(1)
            };
        Components = new List<Component?>();
    }
    #endif

    public void OnClosed()
    {
        InputOutputDevice.InputChanged -= InputOutputDeviceOnInputChanged;
    }

    private int _currentPosition;

    private void InputOutputDeviceOnInputChanged(object? sender, InputChangedEventArgs e)
    {
        if (e.IsPressed)
        {
            _currentPosition = e.Position;
        }
    }
    
    public override Precondition[]? Copy()
    {
        _inputCreator.InputType = InputType;
        _inputCreator.Input = Input;
        _inputCreator.EventType = EventType;
        _inputCreator.PmdgEvent = PmdgEvent;
        _inputCreator.PmdgMousePress = PmdgMousePress;
        _inputCreator.PmdgMouseRelease = PmdgMouseRelease;
        _inputCreator.Event = Event;
        _inputCreator.OnRelease = OnRelease;
        _inputCreator.DataPress = DataPress;
        _inputCreator.DataPress2 = DataPress2;
        _inputCreator.DataRelease = DataRelease;
        _inputCreator.DataRelease2 = DataRelease2;
        return base.Copy();
    }

    [ObservableProperty]
    private string? _inputType;

    private IEnumerable<Component?> GetComponents(string? value)
    {
        return value switch
        {
            ProfileCreatorModel.Switch => InputOutputDevice.Switch.Components,
            _ => Components
        };
    }

    public static string[] InputTypes => [ProfileCreatorModel.Switch];

    [ObservableProperty]
    private IEnumerable<Component?> _components;

    [ObservableProperty]
    private Component? _input;

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
            }

            _eventType = value;
        }
    }

    [ObservableProperty]
    private bool _isMsfsSimConnect;

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
        DataPress2 = null;
        DataRelease2 = null;
        ResetEventType();
    }

    [ObservableProperty]
    private bool _isKEvent;

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
        ResetEventType();
    }

    [ObservableProperty]
    private bool _isRpn;

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
        ResetEventType();
        DataPress = null;
        DataPress2 = null;
        DataRelease = null;
        DataRelease2 = null;
    }

    [ObservableProperty]
    private bool _isPmdg737;

    partial void OnIsPmdg737Changed(bool value)
    {
        if (!value)
        {
            return;
        }

        EventType = ProfileCreatorModel.Pmdg737;
        IsMsfsSimConnect = false;
        IsKEvent = false;
        IsRpn = false;
        Event = null;
        OnRelease = false;
        DataPress2 = null;
        DataRelease2 = null;
    }

    private void ResetEventType()
    {
        PmdgEvent = null;
        ClearPmdgMousePress();
        ClearPmdgMouseRelease();
        OnRelease = false;
    }

    [ObservableProperty]
    private B737.Event? _pmdgEvent;
    
    [ObservableProperty]
    private string? _searchPmdgEvent;
    
    partial void OnSearchPmdgEventChanged(string? value)
    {
        if (Enum.TryParse(value, true, out B737.Event result))
        {
            PmdgEvent = result;
        }
    }

    public static IEnumerable<string?> PmdgEventEnumerable => Enum.GetNames(typeof(B737.Event));
    
    public static Mouse[] PmdgMouseFlags =>
    [
        Mouse.LeftSingle,
        Mouse.LeftRelease,
        Mouse.RightSingle,
        Mouse.RightRelease,
        Mouse.WheelDown,
        Mouse.WheelUp
    ];

    [ObservableProperty]
    private Mouse? _pmdgMousePress;

    [RelayCommand]
    private void ClearPmdgMousePress() => PmdgMousePress = null;

    [ObservableProperty]
    private Mouse? _pmdgMouseRelease;

    [RelayCommand]
    private void ClearPmdgMouseRelease() => PmdgMouseRelease = null;

    [ObservableProperty]
    private string? _event;

    [ObservableProperty]
    private bool _onRelease;

    [ObservableProperty]
    private uint? _dataPress;
    
    [ObservableProperty]
    private uint? _dataPress2;

    [ObservableProperty]
    private uint? _dataRelease;
    
    [ObservableProperty]
    private uint? _dataRelease2;

    [RelayCommand]
    private void GetSwitch()
    {
        Input = Components.FirstOrDefault(i => i?.Position == _currentPosition);
    }
}