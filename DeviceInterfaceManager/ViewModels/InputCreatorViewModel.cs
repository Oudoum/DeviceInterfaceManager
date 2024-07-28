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
        Description = _inputCreator.Description;
        InputType = inputCreator.InputType;
        Components = GetComponents(InputType);
        Input = Components.FirstOrDefault(x => x?.Position == inputCreator.Input?.Position);
        EventType = inputCreator.EventType;
        Event = inputCreator.Event;
        DataPress = inputCreator.DataPress;
        DataPress2 = inputCreator.DataPress2;
        DataRelease = inputCreator.DataRelease;
        DataRelease2 = inputCreator.DataRelease2;
        PmdgEvent = inputCreator.PmdgEvent;
        PmdgMousePress = inputCreator.PmdgMousePress;
        PmdgMouseRelease = inputCreator.PmdgMouseRelease;
        OnRelease = inputCreator.OnRelease;

        if (PmdgEvent is not null)
        {
            SearchPmdgEvent = GetPmdgEventName();
        }

        InputOutputDevice.SwitchPositionChanged += SwitchPositionChanged;
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
        InputOutputDevice.SwitchPositionChanged -= SwitchPositionChanged;
    }

    public bool GetPosition { get; set; }

    private void SwitchPositionChanged(object? sender, SwitchPositionChangedEventArgs e)
    {
        if (GetPosition && e.IsPressed)
        {
            Input = Components.FirstOrDefault(i => i?.Position == e.Position);
        }
    }

    public override Precondition[]? Copy()
    {
        _inputCreator.Description = GetDescription();
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
    
    private string? GetDescription()
    {
        if (!string.IsNullOrEmpty(Description))
        {
            return Description;
        }

        if (Event is not null)
        {
            return Event;
        }

        return PmdgEventName ?? null;
    }
    
    public string? Description { get; set; }

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

                case ProfileCreatorModel.Pmdg777:
                    IsPmdg777 = true;
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
        IsPmdg777 = false;
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
        IsPmdg777 = false;
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
        IsPmdg777 = false;
        ResetEventType();
        DataPress = null;
        DataPress2 = null;
        DataRelease = null;
        DataRelease2 = null;
    }

    [ObservableProperty]
    private bool _isPmdg;

    [ObservableProperty]
    private bool _isPmdg737;

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
    private bool _isPmdg777;

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
        SearchPmdgEvent = null;
        OnRelease = false;
        DataPress2 = null;
        DataRelease2 = null;
    }

    private void ResetEventType()
    {
        IsPmdg = false;
        PmdgEvent = null;
        PmdgEventName = null;
        ClearPmdgMousePress();
        ClearPmdgMouseRelease();
        OnRelease = false;
    }
    
    [ObservableProperty]
    private string? _event;

    partial void OnEventChanged(string? value)
    {
        if (value == string.Empty)
        {
            Event = null;
        }
    }
        
    [ObservableProperty]
    private long? _dataPress;
    
    [ObservableProperty]
    private long? _dataPress2;

    [ObservableProperty]
    private long? _dataRelease;

    [ObservableProperty]
    private long? _dataRelease2;

    public int? PmdgEvent { get; set; }
    
    [ObservableProperty]
    private string? _pmdgEventName;

    [ObservableProperty]
    private string? _searchPmdgEvent;

    partial void OnSearchPmdgEventChanged(string? value)
    {
        if (IsPmdg737 && Enum.TryParse(value, true, out B737.Event pmdg737Event))
        {
            PmdgEvent = (int)pmdg737Event;
            PmdgEventName = value;
            return;
        }
        
        if (IsPmdg777 && Enum.TryParse(value, true, out B777.Event pmdg777Event))
        {
            PmdgEvent = (int)pmdg777Event;
            PmdgEventName = value;
            return;
        }
        
        PmdgEvent = null;
    }

    private string? GetPmdgEventName()
    {
        if (PmdgEvent is null)
        {
            return null;
        }
        
        if (IsPmdg737)
        {
            return Enum.GetName((B737.Event)PmdgEvent);
        }

        if (IsPmdg777)
        {
           return Enum.GetName((B777.Event)PmdgEvent);
        }

        return null;
    }

    [ObservableProperty]
    private IEnumerable<string?>? _pmdgEventEnumerable;

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
    private void ClearPmdgMousePress()
    {
        PmdgMousePress = null;
    }

    [ObservableProperty]
    private Mouse? _pmdgMouseRelease;

    [RelayCommand]
    private void ClearPmdgMouseRelease()
    {
        PmdgMouseRelease = null;
    }

    [ObservableProperty]
    private bool _onRelease;
}