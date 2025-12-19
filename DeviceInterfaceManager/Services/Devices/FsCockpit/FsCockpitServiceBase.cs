using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Exception = System.Exception;

namespace DeviceInterfaceManager.Services.Devices.FsCockpit;

public abstract class FsCockpitServiceBase : DeviceServiceBase
{
    protected readonly Queue<byte> Buffer;
    private readonly FsCockpitSerialPortService _serialPortService;

    protected FsCockpitServiceBase(FsCockpitSerialPortService fsCockpitSerialPortService)
    {
        Icon = (Geometry?)Application.Current!.FindResource("UsbPort");
        _serialPortService = fsCockpitSerialPortService;
        _serialPortService.GetResponse += GetValue;
        DeviceName = PanelModels[_serialPortService.PanelModelPrefix];
        Id = _serialPortService.SerialNumber;
        Buffer = fsCockpitSerialPortService.Buffer;
    }

    public override Task<ConnectionStatus> ConnectAsync(CancellationToken cancellationToken)
    {
        if (PanelModels.All(x => x.Value != DeviceName))
        {
            return Task.FromResult(ConnectionStatus.NotConnected);
        }

        StartupRequest();
        return Task.FromResult(ConnectionStatus.Connected);
    }

    public override Task Disconnect()
    {
        _serialPortService.Disconnect();
        return Task.CompletedTask;
    }

    protected abstract void StartupRequest();

    protected void SendCommand(params byte[] bytes)
    {
        _serialPortService.SendData(bytes);
    }

    protected byte[] GetFrame(int position)
    {
        return _serialPortService.GetFrame(position);
    }

    private static (byte, byte, byte, byte) GetBytes(int position)
    {
        byte byte1 = 0, byte2 = 0, byte3 = 0, byte4 = 0;
        switch (position)
        {
            case < 8:
                byte1 = (byte)(1 << position);
                break;

            case < 16:
                byte2 = (byte)(1 << (position - 8));
                break;

            case < 24:
                byte3 = (byte)(1 << (position - 16));
                break;

            case < 32:
                byte4 = (byte)(1 << (position - 24));
                break;
        }

        return (byte1, byte2, byte3, byte4);
    }

    public override Task SetLedAsync(int position, bool isEnabled)
    {
        (byte byte1, byte byte2, byte byte3, byte byte4) = GetBytes(position);

        if (isEnabled)
        {
            SetLed(byte1, byte2, byte3, byte4);
            return Task.CompletedTask;
        }

        ClearLed(byte1, byte2, byte3, byte4);
        return Task.CompletedTask;
    }

    protected abstract void SetLed(byte byte1, byte byte2, byte byte3, byte byte4);

    protected abstract void ClearLed(byte byte1, byte byte2, byte byte3, byte byte4);

    public override Task SetDatalineAsync(int position, bool isEnabled)
    {
        (byte byte1, byte byte2, byte byte3, byte byte4) = GetBytes(position);

        if (isEnabled)
        {
            SetDataline(byte1, byte2, byte3, byte4);
            return Task.CompletedTask;
        }

        ClearDataline(byte1, byte2, byte3, byte4);
        return Task.CompletedTask;
    }

    protected abstract void SetDataline(byte byte1, byte byte2, byte byte3, byte byte4);

    protected abstract void ClearDataline(byte byte1, byte byte2, byte byte3, byte byte4);

    public override Task SetSevenSegmentAsync(int position, string data)
    {
        SetDisplay((byte)position, data);
        return Task.CompletedTask;
    }

    protected abstract void SetDisplay(byte position, string data);

    protected static byte[] GetDisplayData(Dictionary<char, byte>? displayDigitCodes, string text, out byte decimalDigit)
    {
        decimalDigit = 0;
        int digitIndex = 0;
        int decimalIndex = 0;
        bool decimalDigitFound = false;
        bool digitFound = false;

        byte spaceDigitCode = 32;
        if (displayDigitCodes is not null)
        {
            displayDigitCodes.TryGetValue(' ', out byte data);
            spaceDigitCode = data;
        }

        byte[] digitArray = Enumerable.Repeat(spaceDigitCode, 16).ToArray();

        foreach (char charData in text)
        {
            if (digitIndex < digitArray.Length && charData is not ('.' or ';'))
            {
                //ASCII
                if (displayDigitCodes is null)
                {
                    digitArray[digitIndex++] = (byte)charData;

                    if (digitFound)
                    {
                        decimalIndex++;
                    }

                    digitFound = true;
                    decimalDigitFound = false;
                    continue;
                }

                //Dictionary
                if (displayDigitCodes.TryGetValue(charData, out byte data))
                {
                    digitArray[digitIndex++] = data;

                    if (digitFound)
                    {
                        decimalIndex++;
                    }

                    digitFound = true;
                    decimalDigitFound = false;
                    continue;
                }
            }

            if (digitIndex >= digitArray.Length && charData is not ('.' or ';'))
            {
                break;
            }

            if (decimalDigitFound || !digitFound)
            {
                digitIndex++;
            }

            if (decimalDigitFound)
            {
                decimalIndex++;
            }

            decimalDigit |= (byte)(1 << decimalIndex);

            if (decimalIndex == digitArray.Length - 1)
            {
                break;
            }

            decimalDigitFound = true;
        }

        return digitArray;
    }

    public override Task SetAnalogAsync(int position, double value)
    {
        byte byteValue;
        try
        {
            byteValue = Convert.ToByte(value);
        }
        catch (Exception)
        {
            return Task.CompletedTask;
        }

        SetAnalog((byte)position, byteValue);
        return Task.CompletedTask;
    }

    protected abstract void SetAnalog(byte position, byte value);

    protected bool ProcessAnalogValue(int position, byte[] frame)
    {
        OnAnalogInValueChanged(position, (sbyte)frame[1]);
        return true;
    }

    protected bool ProcessAnalogValueNormal(int position, byte[] frame)
    {
        OnAnalogInValueChanged(position, frame[1]);
        return true;
    }

    protected bool HandleSwitchPositionChange(byte position, byte startPosition, byte endPosition)
    {
        for (int i = startPosition - position; i <= endPosition - position; i++)
        {
            if (i == 0)
            {
                continue;
            }

            OnSwitchPositionChanged(position + i, false);
        }

        OnSwitchPositionChanged(position, true);
        return true;
    }

    protected bool HandleSwitchPositionChange(byte position)
    {
        OnSwitchPositionChanged(position, true);
        OnSwitchPositionChanged(position, false);
        return true;
    }

    protected bool HandleSwitchPositionChange(byte position, ref bool skipNextEncoderFrame)
    {
        if (_serialPortService.HasHighTensionDetents)
        {
            if (skipNextEncoderFrame)
            {
                skipNextEncoderFrame = false;
                return true;
            }

            skipNextEncoderFrame = true;
        }

        OnSwitchPositionChanged(position, true);
        OnSwitchPositionChanged(position, false);
        return true;
    }

    protected abstract bool GetValue(byte responses);

    private const string Airbus = "AB";
    public const string AirbusRmp = Airbus + "01";
    public const string AirbusFcuLite = Airbus + "02";
    public const string AirbusEfisCpt = Airbus + "03";
    public const string AirbusEfisFo = Airbus + "04";
    public const string AirbusMcduLite = Airbus + "05";
    public const string AirbusEcamSw = Airbus + "06";
    public const string AirbusTcasLpfo = Airbus + "07";
    public const string AirbusTpbp = Airbus + "08";
    public const string AirbusAcp = Airbus + "09";
    public const string AirbusWrClp = Airbus + "10";
    public const string AirbusMiniOvh = Airbus + "11";
    public const string AirbusThrottleEsp = Airbus + "15";
    public const string AirbusLeftOvh = Airbus + "16";
    public const string AirbusRightOvh = Airbus + "17";
    public const string AirbusAdirsOvh = Airbus + "18";
    public const string AirbusAirconOvh = Airbus + "19";
    public const string AirbusElecOvh = Airbus + "20";
    public const string AirbusFuelHydFire = Airbus + "21";
    public const string AirbusAbLgsPtiClk = Airbus + "22";
    public const string AirbusN2PfdCpt = Airbus + "24";
    public const string AirbusN2PfdFo = Airbus + "25";
    public const string AirbusAcpPro = Airbus + "28";

    private static Dictionary<string, string> PanelModels { get; } = new()
    {
        { AirbusRmp, "Airbus RMP" },
        { AirbusFcuLite, "Airbus FCU Lite" },
        { AirbusEfisCpt, "Airbus EFIS CPT" },
        { AirbusEfisFo, "Airbus EFIS FO" },
        { AirbusMcduLite, "Airbus MCDU Lite" },
        { AirbusEcamSw, "Airbus ECAM/SW" },
        { AirbusTcasLpfo, "Airbus TCAS/LPFO" },
        { AirbusTpbp, "Airbus TPBP" },
        { AirbusAcp, "Airbus ACP" },
        { AirbusWrClp, "Airbus WR/CLP" },
        { AirbusMiniOvh, "Airbus MOH" },
        { AirbusThrottleEsp, "Airbus Throttle/ESP" },
        { AirbusLeftOvh, "Airbus LOVH" },
        { AirbusRightOvh, "Airbus ROVH" },
        { AirbusAdirsOvh, "Airbus ADIRS" },
        { AirbusAirconOvh, "Airbus AIRCPC" },
        { AirbusElecOvh, "Airbus ELEC" },
        { AirbusFuelHydFire, "Airbus FHYD/FIRE" },
        { AirbusAbLgsPtiClk, "Airbus AB/LGS/PTI/CLK" },
        { AirbusN2PfdCpt, "Airbus N2PFD CPT" },
        { AirbusN2PfdFo, "Airbus N2PFD FO" },
        { AirbusAcpPro, "Airbus ACP Pro" }
    };
}