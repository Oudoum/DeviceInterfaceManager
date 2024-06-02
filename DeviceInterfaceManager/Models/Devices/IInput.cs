using System;

namespace DeviceInterfaceManager.Models.Devices;

public interface IInput
{
    public ComponentInfo Switch { get; }
    
    public event EventHandler<InputChangedEventArgs> InputChanged;
}

public class InputChangedEventArgs(int position, bool isPressed) : EventArgs
{
    public readonly int Position = position;
    public readonly bool IsPressed = isPressed;
}