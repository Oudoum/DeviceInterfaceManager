using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DeviceInterfaceManager.Server;

public class SignalRServerService
{
    private readonly ILogger _logger;

    public SignalRServerService(ILogger<SignalRServerService> logger)
    {
        _logger = logger;
    }

    private IHost? _host;

    public async Task StartAsync(string? ipAddress, int? port, CancellationToken cancellationToken)
    {
        if (!IPAddress.TryParse(ipAddress, out IPAddress? address))
        {
            _logger.LogError("{ipAddress} is not a valid IP-Address. Reverting to default.", ipAddress);
            address = IPAddress.Loopback;
        }

        port ??= 2024;

        _host = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseKestrel()
                    .UseUrls($"http://{address}:{port}")
                    .ConfigureServices(services =>
                    {
                        services.AddSignalR().AddMessagePackProtocol();
                        services.AddCors(options =>
                        {
                            options.AddDefaultPolicy(
                                configurePolicy =>
                                {
                                    configurePolicy
                                        .AllowAnyOrigin()
                                        .AllowAnyHeader()
                                        .AllowAnyMethod();
                                });
                        });
                    })
                    .Configure(app =>
                    {
                        app.UseRouting();
                        app.UseCors();
                        app.UseEndpoints(endpoints => { endpoints.MapHub<DataHub>("/datahub"); });
                    });
            })
            .Build();

        try
        {
            await _host.StartAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred: {Message}", e.Message);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_host is not null)
        {
            await _host.StopAsync(cancellationToken);
        }
    }
}