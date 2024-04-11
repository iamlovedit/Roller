using Asp.Versioning;
using Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.SetupExtensions;

public static class ApiVersionSetup
{
    public static void AddApiVersionSetup(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var versionOptions = configuration.GetSection(VersionOptions.Name).Get<VersionOptions>();
        if (!(versionOptions?.Enable ?? false))
        {
            return;
        }

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader(versionOptions!.HeaderName),
                new MediaTypeApiVersionReader(versionOptions!.ParameterName));
        });

        services.ConfigureOptions<ConfigureSwaggerOptions>();
    }
}