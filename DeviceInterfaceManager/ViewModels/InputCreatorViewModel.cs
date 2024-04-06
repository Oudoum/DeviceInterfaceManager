using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceInterfaceManager.Models;
using DeviceInterfaceManager.Models.Devices;
using DeviceInterfaceManager.Models.SimConnect.MSFS.PMDG.SDK;
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
        Input = inputCreator.Input;
        EventType = inputCreator.EventType;
        PmdgEvent = inputCreator.PmdgEvent;
        PmdgMousePress = inputCreator.PmdgMousePress;
        if (PmdgMousePress is null)
        {
            PmdgMousePressIndex = -1;
        }

        PmdgMouseRelease = inputCreator.PmdgMouseRelease;
        if (PmdgMouseRelease is null)
        {
            PmdgMouseReleaseIndex = -1;
        }

        Event = inputCreator.Event;
        OnRelease = inputCreator.OnRelease;
        DataPress = inputCreator.DataPress;
        DataRelease = inputCreator.DataRelease;

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
        _inputCreator.DataRelease = DataRelease;
        return base.Copy();
    }

    [ObservableProperty]
    private string? _inputType;

    partial void OnInputTypeChanged(string? value)
    {
        Components = value switch
        {
            ProfileCreatorModel.Switch => InputOutputDevice.Switch.Components,
            _ => Components
        };
    }

    public static string[] InputTypes => [ProfileCreatorModel.Switch];

    [ObservableProperty]
    private IEnumerable<Component?>? _components;

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
        DataRelease = null;
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

    public static Func<string?, CancellationToken, Task<IEnumerable<object>>?> AsyncPopulator => (input, token) => input != null ? SearchPmdgEventAsync(input) : null;

    private static async Task<IEnumerable<object>> SearchPmdgEventAsync(string input)
    {
        return await Task.Run(() =>
        {
            return string.IsNullOrEmpty(input)
                ? Enum.GetNames(typeof(B737.Event))
                : Enum.GetNames(typeof(B737.Event)).Where(name => name.ToString().Contains(input, StringComparison.OrdinalIgnoreCase));
        });
    }

    public static string[] PmdgMouseFlags => ["LeftSingle", "LeftRelease", "RightSingle", "RightRelease", "WheelUp", "WheelDown"];

    [ObservableProperty]
    private Mouse? _pmdgMousePress;

    [ObservableProperty]
    private int _pmdgMousePressIndex;

    partial void OnPmdgMousePressIndexChanged(int value)
    {
       PmdgMousePress = PmdgMouseIndexChanged(value);
    }

    private static Mouse? PmdgMouseIndexChanged(int value)
    {
        return value switch
        {
            0 => Mouse.LeftSingle,
            1 => Mouse.LeftRelease,
            2 => Mouse.RightSingle,
            3 => Mouse.RightRelease,
            4 => Mouse.WheelUp,
            5 => Mouse.WheelDown,
            _ => null
        };
    }

    [RelayCommand]
    private void ClearPmdgMousePress()
    {
        PmdgMousePress = null;
        PmdgMousePressIndex = -1;
    }

    [ObservableProperty]
    private Mouse? _pmdgMouseRelease;

    [ObservableProperty]
    private int _pmdgMouseReleaseIndex = -1;

    partial void OnPmdgMouseReleaseIndexChanged(int value)
    {
       PmdgMouseRelease = PmdgMouseIndexChanged(value);
    }

    [RelayCommand]
    private void ClearPmdgMouseRelease()
    {
        PmdgMouseRelease = null;
        PmdgMouseReleaseIndex = -1;
    }

    [ObservableProperty]
    private string? _event;

    [ObservableProperty]
    private bool _onRelease;

    [ObservableProperty]
    private uint? _dataPress;

    [ObservableProperty]
    private uint? _dataRelease;

    [RelayCommand]
    private void GetSwitch()
    {
        Input = Components?.FirstOrDefault(i => i != null && i.Position == _currentPosition);
    }
}