using Infrastructure.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Infrastructure.SetupExtensions;

public static class SerilogSetup
{
    public static void AddSerilogSetup(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var configuration = builder.Configuration;
        var serilogOptions = configuration.GetSection(SerilogOptions.Name).Get<SerilogOptions>();
        if (!(serilogOptions?.Enable ?? false))
        {
            return;
        }

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .WriteTo.Console(theme: AnsiConsoleTheme.Code)
            .Enrich.FromLogContext();

        if (serilogOptions.WriteFile)
        {
            loggerConfiguration =
                loggerConfiguration.WriteTo.File(Path.Combine("logs", "log"), rollingInterval: RollingInterval.Hour);
        }

        if (serilogOptions.SeqOptions?.Enable ?? false)
        {
            loggerConfiguration = loggerConfiguration.WriteTo.Seq(
                configuration["SEQ_URL"] ?? serilogOptions.SeqOptions.Address,
                apiKey: configuration["SEQ_APIKEY"] ?? serilogOptions.SeqOptions.Secret);
        }

        Log.Logger = loggerConfiguration.CreateLogger();

        builder.Services.AddLogging(logBuilder =>
        {
            logBuilder.ClearProviders();
            logBuilder.AddSerilog(Log.Logger);
        });

        builder.Host.UseSerilog(Log.Logger, true);
    }
}