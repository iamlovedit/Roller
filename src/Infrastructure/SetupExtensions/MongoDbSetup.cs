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

        var mongoDbOptions = configuration.GetSection(MongoDbOptions.SectionName).Get<MongoDbOptions>();
        if (mongoDbOptions is null || !mongoDbOptions.Enable)
        {
            return services;
        }

        var host = configuration[MongoDbOptions.MongoHost] ?? mongoDbOptions.Host;
        ArgumentException.ThrowIfNullOrEmpty(host);

        var user = configuration[MongoDbOptions.MongoUser] ?? mongoDbOptions.User;
        ArgumentException.ThrowIfNullOrEmpty(user);

        var password = configuration[MongoDbOptions.MongoPassword] ?? mongoDbOptions.Password;
        ArgumentException.ThrowIfNullOrEmpty(password);

        var port = configuration[MongoDbOptions.MongoPort] ?? mongoDbOptions.Port;
        ArgumentException.ThrowIfNullOrEmpty(port);

        var database = configuration[MongoDbOptions.MongoDatabase] ?? mongoDbOptions.Database;
        ArgumentException.ThrowIfNullOrEmpty(database);
        var connectionString = $"mongodb://{user}:{password}@{host}:{port}/{database}";
        services.TryAddSingleton<IMongoDatabase>(_ =>
            new MongoClient(connectionString)
                .GetDatabase(mongoDbOptions.Database));
        services.TryAddScoped(typeof(IMongoRepositoryBase<,>), typeof(MongoRepositoryBase<,>));
        return services;
    }
}