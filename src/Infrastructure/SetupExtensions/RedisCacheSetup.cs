using Infrastructure.Cache;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Infrastructure.SetupExtensions;

public static class RedisCacheSetup
{
    public static void AddRedisCacheSetup(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IRedisBasketRepository, RedisBasketRepository>();
        services.AddSingleton(provider =>
        {
            var redisConnectionString = $"{configuration["REDIS_HOST"]},password={configuration["REDIS_PASSWORD"]}";
            var redisConfig = ConfigurationOptions.Parse(redisConnectionString, true);
            redisConfig.ResolveDns = true;
            return ConnectionMultiplexer.Connect(redisConfig);
        });
    }
}