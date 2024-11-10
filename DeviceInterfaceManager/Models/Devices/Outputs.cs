namespace DeviceInterfaceManager.Models.Devices;

public class Outputs : IOutputs
{
    public ComponentInfo Led { get; }
    public ComponentInfo Dataline { get; }
    public ComponentInfo SevenSegment { get; }
    public ComponentInfo Analog { get; }

    private Outputs(ComponentInfo led, ComponentInfo dataline, ComponentInfo sevenSegment, ComponentInfo analog)
    {
        Led = led;
        Dataline = dataline;
        SevenSegment = sevenSegment;
        Analog = analog;
    }

    public class Builder : IOutputs
    {
        private static readonly ComponentInfo Default = new(0, 0);

        public ComponentInfo Led { get; private set; } = Default;
        public ComponentInfo Dataline { get; private set; } = Default;
        public ComponentInfo SevenSegment { get; private set; } = Default;
        public ComponentInfo Analog { get; private set; } = Default;

        public Builder SetLedInfo(int first, int last)
        {
            Led = new ComponentInfo(first, last);
            return this;
        }

        public Builder SetLedInfo(ComponentInfo led)
        {
            Led = led;
            return this;
        }
        
        public Builder SetDatalineInfo(int first, int last)
        {
            Dataline = new ComponentInfo(first, last);
            return this;
        }

        public Builder SetDatalineInfo(ComponentInfo dataline)
        {
            Dataline = dataline;
            return this;
        }

        public Builder SetSevenSegmentInfo(int first, int last)
        {
            SevenSegment = new ComponentInfo(first, last);
            return this;
        }

        public Builder SetSevenSegmentInfo(ComponentInfo sevenSegment)
        {
            SevenSegment = sevenSegment;
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

        public Outputs Build()
        {
            return new Outputs(Led, Dataline, SevenSegment, Analog);
        }
    }
}