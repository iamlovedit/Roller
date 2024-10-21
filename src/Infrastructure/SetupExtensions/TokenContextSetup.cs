namespace Roller.Infrastructure.SetupExtensions;

public static class TokenContextSetup
{
    public static IServiceCollection AddRollerTokenContext(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<JsonWebTokenHandler>();
        services.TryAddSingleton<RollerTokenHandler>();
        return services;
    }
}