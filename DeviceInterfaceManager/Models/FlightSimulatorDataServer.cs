using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using DeviceInterfaceManager.Models.FlightSim.MSFS.PMDG.SDK;

namespace DeviceInterfaceManager.Models;

public class FlightSimulatorDataServer
{
    private TcpListener? _tcpListener;
    private TcpClient? _acceptedTcpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { IncludeFields = true };
    private string _ipAddress = "127.0.0.1";
    private int _port = 2024;

    public async Task StartAsync(string? ipAddress, int? port)
    {
        if (ipAddress is not null)
        {
            _ipAddress = ipAddress;
        }

        if (port is not null)
        {
            _port = port.Value;
        }

        if (_tcpListener is null)
        {
            _tcpListener = new TcpListener(IPAddress.Parse(_ipAddress), _port);
            _tcpListener.Start();
            _acceptedTcpClient = await _tcpListener.AcceptTcpClientAsync();
        }
        
        else
        {
            _tcpListener?.Stop();
            _tcpListener = new TcpListener(IPAddress.Parse(_ipAddress), _port);
            _tcpListener.Start();
            _acceptedTcpClient = await _tcpListener.AcceptTcpClientAsync();
        }
    }

    public void SendPmdgCduData(Cdu.Screen cduScreen, Helper.DataRequestId dataRequestId)
    {
        if (_acceptedTcpClient is null)
        {
            return;
        }

        if (!_acceptedTcpClient.Connected)
        {
            Task.Run(() => StartAsync(_ipAddress, _port));
            return;
        }

        byte[] data = JsonSerializer.SerializeToUtf8Bytes(cduScreen, _jsonSerializerOptions);
        
        using (MemoryStream stream = new())
        {
            stream.WriteByte((byte)dataRequestId);
            stream.Write(data);
            NetworkStream networkStream = _acceptedTcpClient.GetStream();
            try
            {
                networkStream.Write(stream.GetBuffer());
            }
            catch (Exception)
            {
                //
            }
        }
    }
}