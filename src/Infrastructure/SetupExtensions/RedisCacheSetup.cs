using Roller.Infrastructure.Options;
using StackExchange.Redis;

namespace Roller.Infrastructure.SetupExtensions;

public static class RedisCacheSetup
{
    public static IServiceCollection AddRedisCacheSetup(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        var redisOptions = configuration.GetSection(RedisOptions.Name).Get<RedisOptions>();
        if (redisOptions is null || !redisOptions.Enable)
        {
            return services;
        }

        services.AddScoped<IRedisBasketRepository, RedisBasketRepository>();
        services.AddSingleton<ConnectionMultiplexer>(_ =>
        {
            var host = configuration["REDIS_HOST"] ?? redisOptions.Host;
            ArgumentException.ThrowIfNullOrEmpty(host);
            var password = configuration["REDIS_PASSWORD"] ?? redisOptions.Password;
            ArgumentException.ThrowIfNullOrEmpty(password);

            var redisConnectionString = $"{host},password={password}";
            var redisConfig = ConfigurationOptions.Parse(redisConnectionString, true);
            redisConfig.ResolveDns = true;
            return ConnectionMultiplexer.Connect(redisConfig);
        });
        return services;
    }
}