using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Roller.Infrastructure.Options;

namespace Roller.Infrastructure.SetupExtensions;

public static class ApiVersionSetup
{
    public static IServiceCollection AddApiVersionSetup(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IApiVersioningBuilder>? configureApiVersioningBuilder = null,
        Action<ApiVersioningOptions>? configureApiVersioningOptions = null,
        Action<ApiExplorerOptions>? configureApiExplorerOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var versionOptions = configuration.GetSection(VersionOptions.Name).Get<VersionOptions>();
        if (versionOptions is null || !versionOptions.Enable)
        {
            return services;
        }

        var builder = services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader(versionOptions!.HeaderName),
                new MediaTypeApiVersionReader(versionOptions!.ParameterName));
            configureApiVersioningOptions?.Invoke(options);
        }).AddApiExplorer(builder =>
        {
            builder.GroupNameFormat = "'v'VVV";
            builder.SubstituteApiVersionInUrl = true;
            configureApiExplorerOptions?.Invoke(builder);
        });
        configureApiVersioningBuilder?.Invoke(builder);
        services.ConfigureOptions<ConfigureSwaggerOptions>();
        return services;
    }
}