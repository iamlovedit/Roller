using Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.SetupExtensions;

public static class CorsSetup
{
    public static void AddCorsSetup(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var corsOptions = configuration.GetSection(CrossOptions.Name).Get<CrossOptions>();
        if (corsOptions?.Enable ?? false)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(corsOptions.PoliyName, policy =>
                {
                    if (corsOptions.AllowAnyHeader)
                    {
                        policy.AllowAnyHeader();
                    }

                    if (corsOptions.AllowAnyMethod)
                    {
                        policy.AllowAnyMethod();
                    }

                    if (corsOptions.AllowAnyOrigin)
                    {
                        policy.AllowAnyOrigin();
                    }
                });
            });
        }
    }
}