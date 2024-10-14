namespace Roller.Infrastructure.SetupExtensions;

public static class TokenContextSetup
{
    public static IServiceCollection AddRollerTokenContext(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<JwtSecurityTokenHandler>();
        services.TryAddSingleton<RollerTokenHandler>();
        services.TryAddSingleton<ITokenBuilder, TokenBuilder>();
        return services;
    }
}