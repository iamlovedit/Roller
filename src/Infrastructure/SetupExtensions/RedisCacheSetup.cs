using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Roller.Infrastructure.Cache;
using Roller.Infrastructure.Options;
using StackExchange.Redis;

namespace Roller.Infrastructure.SetupExtensions;

public static class RedisCacheSetup
{
    public static void AddRedisCacheSetup(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);

        var redisOptions = configuration.GetSection(RedisOptions.Name).Get<RedisOptions>();
        if (!(redisOptions?.Enable ?? false))
        {
            return;
        }

        services.AddScoped<IRedisBasketRepository, RedisBasketRepository>();
        services.AddSingleton<IConnectionMultiplexer>(provider =>
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
    }
}