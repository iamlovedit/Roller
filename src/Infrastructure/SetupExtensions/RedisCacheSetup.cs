using Infrastructure.Cache;
using Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Infrastructure.SetupExtensions;

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
            var redisConnectionString =
                $"{configuration["REDIS_HOST"] ?? redisOptions.Host},password={configuration["REDIS_PASSWORD"] ?? redisOptions.Password}";
            var redisConfig = ConfigurationOptions.Parse(redisConnectionString, true);
            redisConfig.ResolveDns = true;
            return ConnectionMultiplexer.Connect(redisConfig);
        });
    }
}