namespace Roller.Infrastructure.SetupExtensions;

public static class CorsSetup
{
    public static IServiceCollection AddCorsSetup(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var corsOptions = configuration.GetSection(CrossOptions.Name).Get<CrossOptions>();
        if (corsOptions is null || !corsOptions.Enable)
        {
            return services;
        }

        services.AddCors(options =>
        {
            options.AddPolicy(CrossOptions.Name, policy =>
            {
                if (corsOptions.AllowAnyHeader)
                {
                    policy.AllowAnyHeader();
                }
                else
                {
                    policy.WithHeaders(corsOptions.Headers);
                }

                if (corsOptions.AllowAnyMethod)
                {
                    policy.AllowAnyMethod();
                }
                else
                {
                    policy.WithMethods(corsOptions.Methods);
                }

                if (corsOptions.AllowAnyOrigin)
                {
                    policy.AllowAnyOrigin();
                }
                else
                {
                    policy.WithOrigins(corsOptions.Origins);
                }
            });
        });
        return services;
    }
}