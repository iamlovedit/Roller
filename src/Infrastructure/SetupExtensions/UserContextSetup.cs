using Roller.Infrastructure.HttpContextUser;

namespace Roller.Infrastructure.SetupExtensions;

public static class UserContextSetup
{
    public static IServiceCollection AddRollerUserContext<T>(this IServiceCollection services) where T : IEquatable<T>
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddScoped(typeof(IUserContext<T>), typeof(UserContext<T>));
        return services;
    }
}