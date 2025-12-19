using Avalonia.Logging;
using Serilog;

namespace DeviceInterfaceManager;

public class SerilogSink : ILogSink
{
    private readonly ILogger _logger;

    public SerilogSink(ILogger logger)
    {
        _logger = logger;
    }

    public bool IsEnabled(LogEventLevel level, string area)
    {
        return area switch
        {
            "Property" => false,
            "Binding" => false,
            "Animations" => false,
            "Visual" => false,
            "Layout" => false,
            "Control" => false,
            "HotAvalonia" => false,
            "Dialog" => false,
            _ => _logger.IsEnabled(ConvertLogLevel(level))
        };
    }

    public void Log(LogEventLevel level, string area, object? source, string messageTemplate)
    {
        if (IsEnabled(level, area))
        {
            _logger.Write(ConvertLogLevel(level), "{Area}: {Message}", area, messageTemplate);
        }
    }

    public void Log(LogEventLevel level, string area, object? source, string messageTemplate, params object?[] propertyValues)
    {
        if (IsEnabled(level, area))
        {
            _logger.Write(ConvertLogLevel(level), "{Area}: {Message} - {Details}", area, messageTemplate, propertyValues);
        }
    }

    private static Serilog.Events.LogEventLevel ConvertLogLevel(LogEventLevel level)
    {
        return level switch
        {
            LogEventLevel.Verbose => Serilog.Events.LogEventLevel.Verbose,
            LogEventLevel.Debug => Serilog.Events.LogEventLevel.Debug,
            LogEventLevel.Information => Serilog.Events.LogEventLevel.Information,
            LogEventLevel.Warning => Serilog.Events.LogEventLevel.Warning,
            LogEventLevel.Error => Serilog.Events.LogEventLevel.Error,
            LogEventLevel.Fatal => Serilog.Events.LogEventLevel.Fatal,
            _ => Serilog.Events.LogEventLevel.Verbose,
        };
    }
}