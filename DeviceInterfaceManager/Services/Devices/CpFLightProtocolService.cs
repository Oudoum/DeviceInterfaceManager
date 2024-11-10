using System;
using System.Threading.Tasks;
using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.Services.Devices
{
    public class CpFLightProtocolService : DeviceSerialServiceBase
    {
        public CpFLightProtocolService() : base("COM3", 38400)
        {
            Inputs.Builder inputsBuilder = new();
            Outputs.Builder outputsBuilder = new();
            Outputs = outputsBuilder.SetDatalineInfo( new ComponentInfo(
            [
                new Component(1, "MCP ON"),
                new Component(2, "MCP OFF"),
            ])).Build();
        }
        
        public override Task SetLedAsync(int position, bool isEnabled)
        {
            return Task.CompletedTask;
        }

        public override Task SetDatalineAsync(int position, bool isEnabled)
        {
            return Task.CompletedTask;
        }

        public override Task SetSevenSegmentAsync(int position, string data)
        {
            return Task.CompletedTask;
        }

        public override Task SetAnalogAsync(int position, double value)
        {
            return Task.CompletedTask;
        }

        protected override void DataReceived(byte[] data)
        {
            string hex = BitConverter.ToString(data);
        }
        
        
        
        // public required string DeviceName { get; init; }
        //
        // public required string DeviceDescription { get; init; }
        //
        // public required Dictionary<string, string> ButtonInformations { get; init; }
        //
        // public required Dictionary<string, string> LedCommands { get; init; }
        //
        // public required Dictionary<string, string> DatalinesCommands { get; init; }
        //
        // public required Dictionary<string, string> SevenSegmentCommands { get; init; }
        //
        // public static string GetCommand(string command, object value)
        // {
        //     if (value is bool boolValue)
        //     {
        //         return command.Replace('?', (boolValue ? '0' : '1'));
        //     }
        //     return command.Replace("?", value.ToString());
        // }
        //
        // public static Device Mcp =>
        //     new Device
        //     {
        //         DeviceName = "MCP737",
        //
        //         DeviceDescription = "MCP 737 EL | Pro 1/2/3",
        //
        //         ButtonInformations = new Dictionary<string, string>
        //         {
        //             { "K010", "SPD INT" },
        //             { "K012", "ALT INT"},
        //             { "K017", "FD Right ON" },
        //             { "K018", "FD Right OFF" },
        //             { "K019", "A/T ARM ON" },
        //             { "K020", "A/T ARM OFF" },
        //             { "K021", "N1" },
        //             { "K022", "SPEED"},
        //             { "K023", "C/O" },
        //             { "K024", "LVL CHG" },
        //             { "K025", "HDG SEL" },
        //             { "K026", "VNAV" },
        //             { "K027", "LNAV" },
        //             { "K028", "VOR LOC" },
        //             { "K029", "APP" },
        //             { "K030", "ALT HLD" },
        //             { "K031", "V/S" },
        //             { "K032", "CMD A" },
        //             { "K033", "CMD B" },
        //             { "K034", "CWS A" },
        //             { "K035", "CWS B" },
        //             { "K037", "F/D Left ON" },
        //             { "K038", "F/D Left OFF" },
        //             { "K040", "DISENGAGE DOWN" },
        //             { "K041", "DISENGAGE UP" },
        //             { "K121", "TOGA" },
        //         },
        //
        //         LedCommands = new Dictionary<string, string>
        //         {
        //             { "L?117", "FD Right" },
        //             { "L?121", "N1" },
        //             { "L?122", "SPEED" },
        //             { "L?124", "LVL CHG" },
        //             { "L?125", "HDG SEL" },
        //             { "L?126", "VNAV" },
        //             { "L?127", "LNAV" },
        //             { "L?128", "VOR LOC" },
        //             { "L?129", "APP" },
        //             { "L?130", "ALT HLD" },
        //             { "L?131", "V/S" },
        //             { "L?132", "CMD A" },
        //             { "L?133", "CMD B" },
        //             { "L?134", "CWS A" },
        //             { "L?135", "CWS B" },
        //             { "L?137", "FD Left" },
        //         },
        //
        //         DatalinesCommands = new Dictionary<string, string>
        //         {
        //             { "L0097", "A/T DISENGAGE" },
        //             { "L?155", "CO IAS[0] MACH[1]" },
        //             { "L?188", "Test Mode" },
        //             { "L?195", "Battery/Avionic" },
        //             { "L?196", "Overspeed ON[0] OFF[1] | Underspeed ON[2] OFF[3]" },
        //             { "L?198", "Backlight" },
        //             { "X1?02", "SPEED Display" },
        //             { "X1?05", "VERT SPEED Display" },
        //         },
        //
        //         SevenSegmentCommands = new Dictionary<string, string>
        //         {
        //             {   "V01", "COURSE Display Left" },
        //             {   "V02", "SPEED Display" },
        //             {   "V03", "HEADING Display" },
        //             {   "V04", "ALTITUDE Display" },
        //             {   "V05", "VERT SPEED Display" },
        //             {   "V06", "COURSE Left Display" },
        //         },
        //     };
        //
        // public static void CreateJsonFile(string fileName, Device device)
        // {
        //     Directory.CreateDirectory("CPflight");
        //     File.WriteAllText(Path.Combine("CPflight", $"{fileName}.json"), JsonSerializer.Serialize(device, new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull }));
        // }
    }
}