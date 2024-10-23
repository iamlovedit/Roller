using Roller.Infrastructure.Options;
using MongoDB.Driver;
using Roller.Infrastructure.Repository.Mongo;

namespace Roller.Infrastructure.SetupExtensions;

public static class MongoDbSetup
{
    public static IServiceCollection AddMongoDbSetup(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);

        ArgumentNullException.ThrowIfNull(configuration);

        var mongoDbOptions = configuration.GetSection(MongoDbOptions.Name).Get<MongoDbOptions>();
        if (mongoDbOptions is null || !mongoDbOptions.Enable)
        {
            return services;
        }

        services.TryAddScoped<IMongoDatabase>(_ =>
            new MongoClient(mongoDbOptions.ConnectionString).GetDatabase(mongoDbOptions.Database));

        services.TryAddScoped(typeof(IMongoRepositoryBase<,>), typeof(IMongoRepositoryBase<,>));
        return services;
    }
}