using System;
using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.Services.Devices;

public interface IInputService
{
    public Inputs? Inputs { get; }
    
    public event EventHandler<SwitchPositionChangedEventArgs> SwitchPositionChanged;
    public event EventHandler<AnalogValueChangedEventArgs> AnalogValueChanged;
}

public class SwitchPositionChangedEventArgs(int position, bool isPressed) : EventArgs
{
    public readonly int Position = position;
    public readonly bool IsPressed = isPressed;
}

public class AnalogValueChangedEventArgs(int position, double value) : EventArgs
{
    public readonly int Position = position;
    public readonly double Value = value;
}