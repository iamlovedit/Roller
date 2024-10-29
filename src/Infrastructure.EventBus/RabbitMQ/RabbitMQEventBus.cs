using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Polly;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Roller.Infrastructure.EventBus.Subscriptions;

namespace Roller.Infrastructure.EventBus.RabbitMQ;

public class RabbitMQEventBus : IEventBus
{
    private readonly string _exchangeName;
    private readonly string _queueName;
    private readonly int _publishRetryCount = 5;
    private readonly TimeSpan _subscribeRetryTime = TimeSpan.FromSeconds(5);

    private readonly IPersistentConnection _persistentConnection;
    private readonly IEventBusSubscriptionManager _subscriptionsManager;
    private readonly IServiceProvider _serviceProvider;

    private readonly ILogger<RabbitMQEventBus> _logger;

    private IModel? _consumerChannel;

    public RabbitMQEventBus(
        IPersistentConnection persistentConnection,
        IEventBusSubscriptionManager subscriptionsManager,
        IServiceProvider serviceProvider,
        ILogger<RabbitMQEventBus> logger,
        string brokerName,
        string queueName)
    {
        _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
        _subscriptionsManager = subscriptionsManager ?? throw new ArgumentNullException(nameof(subscriptionsManager));
        _serviceProvider = serviceProvider;
        _logger = logger;
        _exchangeName = brokerName ?? throw new ArgumentNullException(nameof(brokerName));
        _queueName = queueName ?? throw new ArgumentNullException(nameof(queueName));

        ConfigureMessageBroker();
    }

    public void Publish<TEvent>(TEvent @event)
        where TEvent : IntegrationEvent
    {
        if (!_persistentConnection.IsConnected)
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
                        "Could not publish event #{EventId} after {Timeout} seconds: {ExceptionMessage}.", @event.Id,
                        $"{timeSpan.TotalSeconds:n1}", exception.Message);
                });

        var eventName = @event.GetType().Name;

        _logger.LogTrace("Creating RabbitMQ channel to publish event #{EventId} ({EventName})...", @event.Id,
            eventName);

        using var channel = _persistentConnection.CreateModel();
        _logger.LogTrace("Declaring RabbitMQ exchange to publish event #{EventId}...", @event.Id);

        channel.ExchangeDeclare(_exchangeName, ExchangeType.Direct);

        var message = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(message);

        policy.Execute(() =>
        {
            var properties = channel.CreateBasicProperties();
            properties.DeliveryMode = 2;

            _logger.LogTrace("Publishing event to RabbitMQ with ID #{EventId}...", @event.Id);

            channel.BasicPublish(_exchangeName, eventName, true, properties, body);
            _logger.LogTrace("Published event with ID #{EventId}.", @event.Id);
        });
    }

    public void Subscribe<TEvent, TEventHandler>()
        where TEvent : IntegrationEvent
        where TEventHandler : IIntegrationEventHandler<TEvent>
    {
        var eventName = _subscriptionsManager.GetEventIdentifier<TEvent>();
        var eventHandlerName = typeof(TEventHandler).Name;

        AddQueueBindForEventSubscription(eventName);

        _logger.LogInformation("Subscribing to event {EventName} with {EventHandler}...", eventName, eventHandlerName);

        _subscriptionsManager.AddSubscription<TEvent, TEventHandler>();
        StartBasicConsume();

        _logger.LogInformation("Subscribed to event {EventName} with {EvenHandler}.", eventName, eventHandlerName);
    }

    public void Unsubscribe<TEvent, TEventHandler>()
        where TEvent : IntegrationEvent
        where TEventHandler : IIntegrationEventHandler<TEvent>
    {
        var eventName = _subscriptionsManager.GetEventIdentifier<TEvent>();

        _logger.LogInformation("Unsubscribing from event {EventName}...", eventName);

        _subscriptionsManager.RemoveSubscription<TEvent, TEventHandler>();

        _logger.LogInformation("Unsubscribed from event {EventName}.", eventName);
    }

    private void ConfigureMessageBroker()
    {
        _consumerChannel = CreateConsumerChannel();
        _subscriptionsManager.OnEventRemoved += SubscriptionManager_OnEventRemoved;
        _persistentConnection.OnReconnectedAfterConnectionFailure +=
            PersistentConnection_OnReconnectedAfterConnectionFailure;
    }

    private IModel CreateConsumerChannel()
    {
        if (!_persistentConnection.IsConnected)
        {
            _persistentConnection.TryConnect();
        }

        _logger.LogTrace("Creating RabbitMQ consumer channel...");

        var channel = _persistentConnection.CreateModel();

        channel.ExchangeDeclare(_exchangeName, ExchangeType.Direct);
        channel.QueueDeclare(_queueName, true, false, false, null);

        channel.CallbackException += (sender, ea) =>
        {
            _logger.LogWarning(ea.Exception, "Recreating RabbitMQ consumer channel...");
            DoCreateConsumerChannel();
        };

        _logger.LogTrace("Created RabbitMQ consumer channel.");


        return channel;
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

        _consumerChannel.BasicConsume(_queueName, false, consumer);

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

        if (!_subscriptionsManager.HasSubscriptionsForEvent(eventName))
        {
            _logger.LogTrace("There are no subscriptions for this event.");
            return;
        }

        var subscriptions = _subscriptionsManager.GetHandlersForEvent(eventName);
        foreach (var subscription in subscriptions)
        {
            var handler = _serviceProvider.GetService(subscription.HandlerType);
            if (handler == null)
            {
                _logger.LogWarning("There are no handlers for the following event: {EventName}", eventName);
                continue;
            }

            var eventType = _subscriptionsManager.GetEventTypeByName(eventName);

            var @event = JsonSerializer.Deserialize(message, eventType);
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
        channel.QueueUnbind(_queueName, _exchangeName, eventName);

        if (_subscriptionsManager.IsEmpty)
        {
            _consumerChannel.Close();
        }
    }

    private void AddQueueBindForEventSubscription(string eventName)
    {
        var containsKey = _subscriptionsManager.HasSubscriptionsForEvent(eventName);
        if (containsKey)
        {
            return;
        }

        if (!_persistentConnection.IsConnected)
        {
            _persistentConnection.TryConnect();
        }

        using var channel = _persistentConnection.CreateModel();
        channel.QueueBind(_queueName, _exchangeName, eventName);
    }

    private void PersistentConnection_OnReconnectedAfterConnectionFailure(object sender, EventArgs e)
    {
        DoCreateConsumerChannel();
        RecreateSubscriptions();
    }

    private void DoCreateConsumerChannel()
    {
        _consumerChannel.Dispose();
        _consumerChannel = CreateConsumerChannel();
        StartBasicConsume();
    }

    private void RecreateSubscriptions()
    {
        var subscriptions = _subscriptionsManager.GetAllSubscriptions();
        _subscriptionsManager.Clear();

        var eventBusType = GetType();

        foreach (var entry in subscriptions)
        {
            foreach (var subscription in entry.Value)
            {
                var genericSubscribe = eventBusType.GetMethod("Subscribe")
                    .MakeGenericMethod(subscription.EventType, subscription.HandlerType);
                genericSubscribe.Invoke(this, null);
            }
        }
    }
}