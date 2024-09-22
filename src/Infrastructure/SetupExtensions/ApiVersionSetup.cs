using Asp.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Roller.Infrastructure.Options;

namespace Roller.Infrastructure.SetupExtensions;

public static class ApiVersionSetup
{
    public static IServiceCollection AddApiVersionSetup(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var versionOptions = configuration.GetSection(VersionOptions.Name).Get<VersionOptions>();
        if (versionOptions is null || !versionOptions.Enable)
        {
            return services;
        }

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader(versionOptions!.HeaderName),
                new MediaTypeApiVersionReader(versionOptions!.ParameterName));
        }).AddApiExplorer(builder =>
        {
            builder.GroupNameFormat = "'v'VVV";
            builder.SubstituteApiVersionInUrl = true;
        });
        services.ConfigureOptions<ConfigureSwaggerOptions>();
        return services;
    }
}