using Roller.Infrastructure.HttpContextUser;

namespace Roller.Infrastructure.SetupExtensions;

public static class UserContextSetup
{
    public static IServiceCollection AddRollerUserContext(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddScoped<IUserContext, UserContext>();
        return services;
    }
}