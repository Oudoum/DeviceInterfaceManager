namespace DeviceInterfaceManager.Models.Devices;

public class Inputs : IInputs
{
    public ComponentInfo Switch { get; }
    public ComponentInfo Analog { get; }

    private Inputs(ComponentInfo @switch, ComponentInfo analog)
    {
        Switch = @switch;
        Analog = analog;
    }

    public class Builder : IInputs
    {
        private static readonly ComponentInfo Default = new(0, 0);

        public ComponentInfo Switch { get; private set; } = Default;
        public ComponentInfo Analog { get; private set; } = Default;

        public Builder SetSwitchInfo(int first, int last)
        {
            Switch = new ComponentInfo(first, last);
            return this;
        }

        public Builder SetSwitchInfo(ComponentInfo @switch)
        {
            Switch = @switch;
            return this;
        }

        public Builder SetAnalogInfo(int first, int last)
        {
            Analog = new ComponentInfo(first, last);
            return this;
        }

        public Builder SetAnalogInfo(ComponentInfo analog)
        {
            Analog = analog;
            return this;
        }

        public Inputs Build()
        {
            return new Inputs(Switch, Analog);
        }
    }
}