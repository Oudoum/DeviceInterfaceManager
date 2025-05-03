using System;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Timers;

namespace DeviceInterfaceManager.Services.Devices;

public abstract class SerialPortService
{
    private readonly SerialPort _serialPort;
    private readonly Timer _timer = new(1);
    private readonly ConcurrentQueue<byte[]> _framesQueue = new();
    private bool _isProcessing;

    protected SerialPortService(string portName, int baudRate, bool rtsEnable = false)
    {
        _serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);
        _serialPort.RtsEnable = rtsEnable;
    }

    protected bool Connect()
    {
        try
        {
            _serialPort.Open();
        }
        catch (Exception)
        {
            return false;
        }

        _serialPort.DataReceived += OnDataReceived;
        _timer.Elapsed += OnTimedEvent;
        return true;
    }

    public void Disconnect()
    {
        _timer.Elapsed -= OnTimedEvent;
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
        byte[] frame = new byte[bytesToRead];
        _serialPort.Read(frame, 0, bytesToRead);
        DataReceived(frame);
    }

    protected abstract void DataReceived(byte[] frame);

    public void SendData(params byte[] frame)
    {
        _framesQueue.Enqueue(frame);
        if (!_isProcessing)
        {
            ProcessQueue();
        }
    }

    private void ProcessQueue()
    {
        if (!_framesQueue.TryDequeue(out byte[]? frame))
        {
            _isProcessing = false;
            return;
        }

        _serialPort.Write(frame, 0, frame.Length);
        _isProcessing = true;
        _timer.Start();
    }

    private void OnTimedEvent(object? sender, ElapsedEventArgs e)
    {
        _timer.Stop();
        ProcessQueue();
    }

    public void SendData(string text)
    {
        _serialPort.WriteLine(text);
    }
}