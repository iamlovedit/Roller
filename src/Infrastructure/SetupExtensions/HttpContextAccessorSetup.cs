namespace Roller.Infrastructure.SetupExtensions;

public static class HttpContextAccessorSetup
{
    public static IServiceCollection AddHttpContextAccessor(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddScoped<IHttpContextAccessor, HttpContextAccessor>();
        return services;
    }
}