namespace DeviceInterfaceManager.ViewModels.Dialogs;

public class SelectSerialPortDialogModel : SelectConnectionDialogModel
{
    public static string[] PortNames
    {
        get
        {
            string[] portNames = new string[99];
            for (int i = 0; i < 99; i++)
            {
                portNames[i] = $"COM{i + 1}";
            }

            return portNames;
        }
    }
}