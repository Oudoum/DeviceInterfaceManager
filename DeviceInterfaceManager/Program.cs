using Avalonia;
using System;
using Avalonia.Logging;
using Serilog;
using Velopack;
using LogEventLevel = Serilog.Events.LogEventLevel;

namespace DeviceInterfaceManager;

internal static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        VelopackApp.Build().Run();

        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Application terminated unexpectedly!");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(LogEventLevel.Warning)
            .WriteTo.File(App.UserDataPath + @"\Logs\Log.txt", LogEventLevel.Warning, rollingInterval: RollingInterval.Day)
            .Filter.ByExcluding(logEvent => logEvent.MessageTemplate.Text.Contains("binding"))
            .CreateLogger();

        Log.Information("Starting up");

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToSink(Log.Logger);
    }

    private static AppBuilder LogToSink(this AppBuilder builder, ILogger logger)
    {
        Logger.Sink = new SerilogSink(logger);
        return builder;
    }
}