using System;

namespace DeviceInterfaceManager.Devices;

public interface IInput
{
    ComponentInfo Switch { get; }
    
    event EventHandler<InputChangedEventArgs> InputChanged;
}

public class InputChangedEventArgs(int position, bool isPressed) : EventArgs
{
    public int Position = position;
    public bool IsPressed = isPressed;
}