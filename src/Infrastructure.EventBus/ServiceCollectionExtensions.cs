using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Roller.Infrastructure.EventBus.RabbitMQ;
using Roller.Infrastructure.EventBus.Subscriptions;

namespace Roller.Infrastructure.EventBus;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRollerRabbitMQEventBus(this IServiceCollection services,
        string connectionUrl, string exchangeName, string queueName, int timeoutBeforeReconnecting = 15)
    {
        ArgumentNullException.ThrowIfNull(connectionUrl);
        ArgumentNullException.ThrowIfNull(exchangeName);
        ArgumentNullException.ThrowIfNull(queueName);
        ArgumentNullException.ThrowIfNull(services);


        services.AddSingleton<IEventBusSubscriptionManager, InMemoryEventBusSubscriptionManager>();
        services.AddSingleton<IPersistentConnection, RabbitMQPersistentConnection>(factory =>
        {
            var connectionFactory = new ConnectionFactory
            {
                Uri = new Uri(connectionUrl),
                DispatchConsumersAsync = true,
            };

            var logger = factory.GetService<ILogger<RabbitMQPersistentConnection>>();
            return new RabbitMQPersistentConnection(connectionFactory, logger, timeoutBeforeReconnecting);
        });
        services.AddSingleton<IEventBus, RabbitMQEventBus>(factory =>
        {
            var persistentConnection = factory.GetService<IPersistentConnection>();
            var subscriptionManager = factory.GetService<IEventBusSubscriptionManager>();
            var logger = factory.GetService<ILogger<RabbitMQEventBus>>();

            return new RabbitMQEventBus(persistentConnection, logger, factory,
                subscriptionManager, exchangeName,queueName);
        });
        return services;
    }
}