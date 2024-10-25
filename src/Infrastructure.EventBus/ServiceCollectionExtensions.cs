using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Roller.Infrastructure.EventBus.RabbitMQ;
using Roller.Infrastructure.EventBus.Subscriptions;

namespace Roller.Infrastructure.EventBus;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRollerRabbitMQEventBus(this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(services);

        var rabbitMqOptions = configuration.GetSection(RabbitMqConnectionOptions.SectionName)
            .Get<RabbitMqConnectionOptions>();
        ArgumentNullException.ThrowIfNull(rabbitMqOptions);
        services.AddSingleton<IEventBusSubscriptionManager, InMemoryEventBusSubscriptionManager>();
        services.AddSingleton<IPersistentConnection, RabbitMQPersistentConnection>(factory =>
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = configuration[RabbitMqConnectionOptions.HOST] ?? rabbitMqOptions.HostName,
                UserName = configuration[RabbitMqConnectionOptions.USER] ?? rabbitMqOptions.Username,
                Password = configuration[RabbitMqConnectionOptions.PASSWORD] ?? rabbitMqOptions.Password,
                DispatchConsumersAsync = rabbitMqOptions.DispatchConsumersAsync,
            };

            var logger = factory.GetService<ILogger<RabbitMQPersistentConnection>>();
            return new RabbitMQPersistentConnection(connectionFactory, logger,
                rabbitMqOptions.TimeoutBeforeReconnecting);
        });
        services.AddSingleton<IEventBus, RabbitMQEventBus>(factory =>
        {
            var persistentConnection = factory.GetService<IPersistentConnection>();
            var subscriptionManager = factory.GetService<IEventBusSubscriptionManager>();
            var logger = factory.GetService<ILogger<RabbitMQEventBus>>();

            return new RabbitMQEventBus(persistentConnection, subscriptionManager, factory, logger,
                rabbitMqOptions.ExchangeName, rabbitMqOptions.QueueName);
        });
        return services;
    }

    public static void SubscribeEvent<TEvent, TEvenHandler>
        (this  IServiceProvider  services)
        where TEvent : IntegrationEvent
        where TEvenHandler : IIntegrationEventHandler<TEvent>
    {
        ArgumentNullException.ThrowIfNull(services);
        var eventBus = services.GetRequiredService<IEventBus>();
        eventBus.Subscribe<TEvent, TEvenHandler>();
    }
}