using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace DeviceInterfaceManager.Services.Devices;

public abstract class DeviceSerialServiceBase : DeviceServiceBase
{
    private readonly SerialPort _serialPort;

    protected DeviceSerialServiceBase(string portName, int baudRate, bool hasRts = false)
    {
        Icon = (Geometry?)Application.Current!.FindResource("UsbPort");
        _serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);
        _serialPort.RtsEnable = hasRts;
    }

    public override Task<ConnectionStatus> ConnectAsync(CancellationToken cancellationToken)
    {
        try
        {
            _serialPort.Open();
        }
        catch (Exception)
        {
            Disconnect();
            return Task.FromResult(ConnectionStatus.NotConnected);
        }

        _serialPort.DataReceived += OnDataReceived;
        return Task.FromResult(ConnectionStatus.Connected);
    }

    public override void Disconnect()
    {
        _serialPort.DataReceived -= OnDataReceived;
        
        try
        {
            _serialPort.Close();
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        if (_serialPort is not { IsOpen: true, BytesToRead: > 0 })
        {
            return;
        }

        int bytesToRead = _serialPort.BytesToRead;
        byte[] data = new byte[bytesToRead];
        _serialPort.Read(data, 0, bytesToRead);
        DataReceived(data);
    }

    protected abstract void DataReceived(byte[] data);

    protected void SendData(params byte[] data)
    {
        _serialPort.Write(data, 0, data.Length);
    }

    protected void SendData(string text)
    {
        _serialPort.WriteLine(text);
    }
}