using System;

namespace DeviceInterfaceManager.Models.Devices;

public interface IInput
{
    ComponentInfo Switch { get; }
    
    event EventHandler<InputChangedEventArgs> InputChanged;
}

public class InputChangedEventArgs(int position, bool isPressed) : EventArgs
{
    public readonly int Position = position;
    public readonly bool IsPressed = isPressed;
}