using System;

namespace DeviceInterfaceManager.Models.Devices;

public interface IInput
{
    public ComponentInfo Switch { get; }
    public ComponentInfo AnalogIn { get; }

    public event EventHandler<SwitchPositionChangedEventArgs> SwitchPositionChanged;
    public event EventHandler<AnalogInValueChangedEventArgs> AnalogInValueChanged;
}

public class SwitchPositionChangedEventArgs(int position, bool isPressed) : EventArgs
{
    public readonly int Position = position;
    public readonly bool IsPressed = isPressed;
}

public class AnalogInValueChangedEventArgs(int position, int value) : EventArgs
{
    public readonly int Position = position;
    public readonly int Value = value;
}