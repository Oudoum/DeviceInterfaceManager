using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DeviceInterfaceManager.Server;

public class SignalRServerService
{
    private IHost? _host;
    public async Task StartAsync(string? ipAddress, int? port, CancellationToken cancellationToken)
    {
        if (!IPAddress.TryParse(ipAddress, out IPAddress? address))
        {
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
        
        await _host.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_host is not null)
        {
            await _host.StopAsync(cancellationToken);
        }
    }
}