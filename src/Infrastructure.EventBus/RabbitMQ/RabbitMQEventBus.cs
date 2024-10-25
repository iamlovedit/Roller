using System.Net.Sockets;
using System.Text;
using Polly;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Roller.Infrastructure.EventBus.Subscriptions;

namespace Roller.Infrastructure.EventBus.RabbitMQ;

public class RabbitMQEventBus : IEventBus
{
    private readonly IPersistentConnection _persistentConnection;
    private readonly ILogger<RabbitMQEventBus> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventBusSubscriptionManager _eventBusSubscriptionManager;
    private readonly string _exchangeName;
    private readonly string _queueName;

    public RabbitMQEventBus(IPersistentConnection persistentConnection,
        ILogger<RabbitMQEventBus> logger,
        IServiceProvider serviceProvider,
        IEventBusSubscriptionManager eventBusSubscriptionManager,
        string exchangeName,
        string queueName)
    {
        _persistentConnection = persistentConnection;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _eventBusSubscriptionManager = eventBusSubscriptionManager;
        _exchangeName = exchangeName;
        _queueName = queueName;
        ConfigureMessageBroker();
    }

    private readonly int _publishRetryCount = 5;
    private IModel _consumerChannel;
    private readonly TimeSpan _subscribeRetryTime = TimeSpan.FromSeconds(5);

    public void Publish(IntegrationEvent integrationEvent)
    {
        if (_persistentConnection.IsConnected)
        {
            _persistentConnection.TryConnect();
        }

        var policy = Policy
            .Handle<BrokerUnreachableException>()
            .Or<SocketException>()
            .WaitAndRetry(_publishRetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan) =>
                {
                    _logger.LogWarning(exception,
                        "Could not publish event #{EventId} after {Timeout} seconds: {ExceptionMessage}.",
                        integrationEvent.Id,
                        $"{timeSpan.TotalSeconds:n1}", exception.Message);
                });
        var eventName = integrationEvent.GetType().Name;
        _logger.LogTrace("Creating RabbitMQ channel to publish event #{EventId} ({EventName})...", integrationEvent.Id,
            eventName);
        using var channel = _persistentConnection.CreateModel();
        _logger.LogTrace("Declaring RabbitMQ exchange to publish event #{EventId}...", integrationEvent.Id);
        channel.ExchangeDeclare(exchange: _exchangeName, type: "direct");

        var message = JsonConvert.SerializeObject(integrationEvent);
        var body = Encoding.UTF8.GetBytes(message);
        policy.Execute(() =>
        {
            var properties = channel.CreateBasicProperties();
            properties.DeliveryMode = 2;
            _logger.LogTrace("Publishing event to RabbitMQ with ID #{EventId}...", integrationEvent.Id);
            channel.BasicPublish(
                exchange: _exchangeName,
                routingKey: eventName,
                mandatory: true,
                basicProperties: properties,
                body: body);
            _logger.LogTrace("Published event with ID #{EventId}.", integrationEvent.Id);
        });
    }

    public void Subscribe<TEvent, TEventHandler>() where TEvent : IntegrationEvent
        where TEventHandler : IIntegrationEventHandler<TEvent>
    {
        var eventName = _eventBusSubscriptionManager.GetEventIdentifier<TEvent>();
        var eventHandlerName = typeof(TEventHandler).Name;
        AddQueueBindForEventSubscription(eventName);
        _logger.LogInformation("Subscribing to event {EventName} with {EventHandler}...", eventName, eventHandlerName);
        _eventBusSubscriptionManager.AddSubscription<TEvent, TEventHandler>();
        _logger.LogInformation("Subscribed to event {EventName} with {EvenHandler}.", eventName, eventHandlerName);
    }

    public void Unsubscribe<TEvent, TEventHandler>() where TEvent : IntegrationEvent
        where TEventHandler : IIntegrationEventHandler<TEvent>
    {
        var eventName = _eventBusSubscriptionManager.GetEventIdentifier<TEvent>();
        _logger.LogInformation("Unsubscribing from event {EventName}...", eventName);

        _eventBusSubscriptionManager.RemoveSubscription<TEvent, TEventHandler>();

        _logger.LogInformation("Unsubscribed from event {EventName}.", eventName);
    }


    private void ConfigureMessageBroker()
    {
        _consumerChannel = CreateConsumerChannel();
        _eventBusSubscriptionManager.OnEventRemoved += SubscriptionManager_OnEventRemoved;
        _persistentConnection.OnReconnectedAfterConnectionFailure +=
            PersistentConnection_OnReconnectedAfterConnectionFailure;
    }

    private void PersistentConnection_OnReconnectedAfterConnectionFailure(object sender, EventArgs e)
    {
        DoCreateConsumerChannel();
        RecreateSubscriptions();
    }

    private void RecreateSubscriptions()
    {
        var subscriptions = _eventBusSubscriptionManager.GetAllSubscriptions();
        _eventBusSubscriptionManager.Clear();

        var eventBusType = this.GetType();

        foreach (var entry in subscriptions)
        {
            foreach (var genericSubscribe in entry.Value.Select(subscription => eventBusType.GetMethod("Subscribe")
                         .MakeGenericMethod(subscription.EventType, subscription.HandlerType)))
            {
                genericSubscribe.Invoke(this, null);
            }
        }
    }

    private void DoCreateConsumerChannel()
    {
        _consumerChannel.Dispose();
        _consumerChannel = CreateConsumerChannel();
        StartBasicConsume();
    }

    private void StartBasicConsume()
    {
        _logger.LogTrace("Starting RabbitMQ basic consume...");

        if (_consumerChannel == null)
        {
            _logger.LogError("Could not start basic consume because consumer channel is null.");
            return;
        }

        var consumer = new AsyncEventingBasicConsumer(_consumerChannel);
        consumer.Received += Consumer_Received;

        _consumerChannel.BasicConsume
        (
            queue: _queueName,
            autoAck: false,
            consumer: consumer
        );

        _logger.LogTrace("Started RabbitMQ basic consume.");
    }

    private async Task Consumer_Received(object sender, BasicDeliverEventArgs eventArgs)
    {
        var eventName = eventArgs.RoutingKey;
        var message = Encoding.UTF8.GetString(eventArgs.Body.Span);

        var isAcknowledged = false;

        try
        {
            await ProcessEvent(eventName, message);

            _consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
            isAcknowledged = true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error processing the following message: {Message}.", message);
        }
        finally
        {
            if (!isAcknowledged)
            {
                await TryEnqueueMessageAgainAsync(eventArgs);
            }
        }
    }


    private async Task TryEnqueueMessageAgainAsync(BasicDeliverEventArgs eventArgs)
    {
        try
        {
            _logger.LogWarning("Adding message to queue again with {Time} seconds delay...",
                $"{_subscribeRetryTime.TotalSeconds:n1}");

            await Task.Delay(_subscribeRetryTime);
            _consumerChannel.BasicNack(eventArgs.DeliveryTag, false, true);

            _logger.LogTrace("Message added to queue again.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Could not enqueue message again: {Error}.", ex.Message);
        }
    }

    private async Task ProcessEvent(string eventName, string message)
    {
        _logger.LogTrace("Processing RabbitMQ event: {EventName}...", eventName);

        if (!_eventBusSubscriptionManager.HasSubscriptionsForEvent(eventName))
        {
            _logger.LogTrace("There are no subscriptions for this event.");
            return;
        }

        var subscriptions = _eventBusSubscriptionManager.GetHandlersForEvent(eventName);
        foreach (var subscription in subscriptions)
        {
            var handler = _serviceProvider.GetService(subscription.HandlerType);
            if (handler == null)
            {
                _logger.LogWarning("There are no handlers for the following event: {EventName}", eventName);
                continue;
            }

            var eventType = _eventBusSubscriptionManager.GetEventTypeByName(eventName);

            var @event = JsonConvert.DeserializeObject(message, eventType);
            var eventHandlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
            await Task.Yield();
            await (Task)eventHandlerType.GetMethod(nameof(IIntegrationEventHandler<IntegrationEvent>.HandleAsync))
                .Invoke(handler, [@event]);
        }

        _logger.LogTrace("Processed event {EventName}.", eventName);
    }

    private void SubscriptionManager_OnEventRemoved(object sender, string eventName)
    {
        if (!_persistentConnection.IsConnected)
        {
            _persistentConnection.TryConnect();
        }

        using var channel = _persistentConnection.CreateModel();
        channel.QueueUnbind(queue: _queueName, exchange: _exchangeName, routingKey: eventName);

        if (_eventBusSubscriptionManager.IsEmpty)
        {
            _consumerChannel.Close();
        }
    }

    private IModel CreateConsumerChannel()
    {
        if (!_persistentConnection.IsConnected)
        {
            _persistentConnection.TryConnect();
        }

        _logger.LogTrace("Creating RabbitMQ consumer channel...");

        var channel = _persistentConnection.CreateModel();

        channel.ExchangeDeclare(exchange: _exchangeName, type: "direct");
        channel.QueueDeclare
        (
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        channel.CallbackException += (_, ea) =>
        {
            _logger.LogWarning(ea.Exception, "Recreating RabbitMQ consumer channel...");
            DoCreateConsumerChannel();
        };

        _logger.LogTrace("Created RabbitMQ consumer channel.");
        return channel;
    }

    private void AddQueueBindForEventSubscription(string eventName)
    {
        var containsKey = _eventBusSubscriptionManager.HasSubscriptionsForEvent(eventName);
        if (containsKey)
        {
            return;
        }

        if (!_persistentConnection.IsConnected)
        {
            _persistentConnection.TryConnect();
        }

        using var channel = _persistentConnection.CreateModel();
        channel.QueueBind(queue: _queueName, exchange: _exchangeName, routingKey: eventName);
    }
}