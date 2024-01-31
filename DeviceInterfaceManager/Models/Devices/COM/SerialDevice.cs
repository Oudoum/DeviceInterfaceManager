using System;
using System.Diagnostics;
using System.IO.Ports;

namespace DeviceInterfaceManager.Models.Devices.COM;

public class SerialDevice
{
    private string PortName { get; set; } = "COM3";

    private int BaudRate { get; set; } = 9600;

    private Parity Parity { get; set; } = Parity.None;

    private int DataBits { get; set; } = 8;

    private StopBits StopBits { get; set; } = StopBits.One;

    SerialPort _serialPort;

    public SerialDevice()
    {

        _serialPort = new SerialPort(PortName, BaudRate, Parity, DataBits, StopBits)
        {
            NewLine = "\r\n"
        };
        _serialPort.DataReceived += SerialPort_DataReceived;
        _serialPort.ErrorReceived += SerialPort_ErrorReceived;
        _serialPort.PinChanged += SerialPort_PinChanged;
        _serialPort.Disposed += SerialPort_Disposed;

        try
        {
            _serialPort.Open();
        }
        catch (Exception)
        {
            _serialPort.DataReceived -= SerialPort_DataReceived;
            _serialPort.ErrorReceived -= SerialPort_ErrorReceived;
            _serialPort.PinChanged -= SerialPort_PinChanged;
            _serialPort.Disposed -= SerialPort_Disposed;
            _serialPort.Close();
        }

        //serialPort.WriteLine("LED:13:1");
        //Thread.Sleep(TimeSpan.FromSeconds(3));
        //serialPort.WriteLine("LED:13:0");
        //Thread.Sleep(TimeSpan.FromSeconds(3));
        //serialPort.WriteLine("LED:13:1");
        //Thread.Sleep(TimeSpan.FromSeconds(3));
        //serialPort.WriteLine("LED:13:0");

        _serialPort.WriteLine("START");
        //Thread.Sleep(TimeSpan.FromSeconds(3));
        //Thread.Sleep(TimeSpan.FromSeconds(3));
        //Thread.Sleep(TimeSpan.FromSeconds(3));
        //Thread.Sleep(TimeSpan.FromSeconds(3));
        //Thread.Sleep(TimeSpan.FromSeconds(3));
        //Thread.Sleep(TimeSpan.FromSeconds(3));

        //serialPort.DataReceived -= SerialPort_DataReceived;
        //serialPort.ErrorReceived -= SerialPort_ErrorReceived;
        //serialPort.PinChanged -= SerialPort_PinChanged;
        //serialPort.Disposed -= SerialPort_Disposed;
        //serialPort.Close();
    }
    
    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        if (!_serialPort.IsOpen)
            return;
        
        string[] data = _serialPort.ReadLine().Split(':');
        string type = data[0];
        switch (data.Length)
        {
            case 1:
                Debug.WriteLine(type);
                break;
            case 3:
            {
                string pin = data[1];
                string value = data[2];
                switch (type)
                {
                    case "SW":
                        break;
                }
                Debug.WriteLine(type + pin + value);
                break;
            }
        }
    }
    
    private void SerialPort_Disposed(object? sender, EventArgs e)
    {
        
    }

    private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
    {

    }

    private void SerialPort_PinChanged(object sender, SerialPinChangedEventArgs e)
    {

    }
}