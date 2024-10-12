﻿using Microsoft.Extensions.Logging;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Roller.Infrastructure.SetupExtensions;

public static class SerilogSetup
{
    public static IServiceCollection AddSerilogSetup(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        var serilogOptions = configuration.GetSection(SerilogOptions.Name).Get<SerilogOptions>();
        if (serilogOptions is null || !serilogOptions.Enable)
        {
            return services;
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
            var serverUrl = configuration["SEQ_URL"] ?? serilogOptions.SeqOptions.Address;
            ArgumentException.ThrowIfNullOrEmpty(serverUrl);
            var apiKey = configuration["SEQ_APIKEY"] ?? serilogOptions.SeqOptions.Secret;
            ArgumentException.ThrowIfNullOrEmpty(apiKey);
            loggerConfiguration = loggerConfiguration.WriteTo.Seq(
                configuration["SEQ_URL"] ?? serilogOptions.SeqOptions.Address,
                apiKey: configuration["SEQ_APIKEY"] ?? serilogOptions.SeqOptions.Secret);
        }

        Log.Logger = loggerConfiguration.CreateLogger();
        services.AddLogging(logBuilder =>
        {
            logBuilder.ClearProviders();
            logBuilder.AddSerilog(Log.Logger);
        });
        return services;
    }
}