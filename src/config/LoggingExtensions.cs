using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Warbreaker.Configuration;

public static class LoggingExtensions
{
    public static ILoggingBuilder AddAppLogging(this ILoggingBuilder builder, ConfigurationManager configs, string environmentName)
    {
        if (string.Equals(environmentName, Constants.DEVELOPMENT_ENVIRONMENT_NAME, StringComparison.OrdinalIgnoreCase))
        {
            Serilog.Log.Logger = new LoggerConfiguration()
                .WriteTo
                    .Console()
                .WriteTo
                    .Debug()
                .MinimumLevel
                    .Is(LogEventLevel.Verbose)
                .CreateLogger();
        }
        else
        {
            string? aiKey = configs.GetValue<string>(Constants.APP_INSIGHTS_KEY_NAME);
            if (string.IsNullOrEmpty(aiKey))
            {
                throw new InvalidOperationException($"Could not find App Insights Telemetry Key in Config Store: `{Constants.APP_INSIGHTS_KEY_NAME}`");
            }

            Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration config = new Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration(aiKey);
            Serilog.Log.Logger = new LoggerConfiguration()
                .WriteTo
                    .ApplicationInsights(config, TelemetryConverter.Traces)
                .MinimumLevel
                    .Is(LogEventLevel.Information)
                .CreateLogger();
        }

        builder.AddSerilog();

        return builder;
    }
}
