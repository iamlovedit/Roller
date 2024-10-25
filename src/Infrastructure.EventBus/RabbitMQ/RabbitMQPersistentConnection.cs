using System.Net.Sockets;
using System.Text;
using Polly;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Roller.Infrastructure.EventBus.RabbitMQ;

public class RabbitMQPersistentConnection(
    IConnectionFactory connectionFactory,
    ILogger<RabbitMQPersistentConnection> logger,
    int timeoutBeforeReconnecting = 15)
    : IPersistentConnection
{
    private IConnection _connection;
    private bool _disposed;
    private readonly object _locker = new();
    private bool _connectionFailed;
    private readonly TimeSpan _timeoutBeforeReconnecting = TimeSpan.FromSeconds(timeoutBeforeReconnecting);
    public event EventHandler? OnReconnectedAfterConnectionFailure;
    public bool IsConnected => _connection is { IsOpen: true } && !_disposed;

    public bool TryConnect()
    {
        logger.LogInformation("Trying to connect to RabbitMQ...");

        lock (_locker)
        {
            // Creates a policy to retry connecting to message broker until it succeds.
            var policy = Policy
                .Handle<SocketException>()
                .Or<BrokerUnreachableException>()
                .WaitAndRetryForever((duration) => _timeoutBeforeReconnecting,
                    (ex, time) =>
                    {
                        logger.LogWarning(ex,
                            "RabbitMQ Client could not connect after {TimeOut} seconds ({ExceptionMessage}). Waiting to try again...",
                            $"{(int)time.TotalSeconds}", ex.Message);
                    });

            policy.Execute(() => { _connection = connectionFactory.CreateConnection(); });

            if (!IsConnected)
            {
                logger.LogCritical("ERROR: could not connect to RabbitMQ.");
                _connectionFailed = true;
                return false;
            }

            // These event handlers hanle situations where the connection is lost by any reason. They try to reconnect the client.
            _connection.ConnectionShutdown += OnConnectionShutdown;
            _connection.CallbackException += OnCallbackException;
            _connection.ConnectionBlocked += OnConnectionBlocked;
            _connection.ConnectionUnblocked += OnConnectionUnblocked;

            logger.LogInformation(
                "RabbitMQ Client acquired a persistent connection to '{HostName}' and is subscribed to failure events",
                _connection.Endpoint.HostName);

            // If the connection has failed previously because of a RabbitMQ shutdown or something similar, we need to guarantee that the exchange and queues exist again.
            // It's also necessary to rebind all application event handlers. We use this event handler below to do this.
            if (_connectionFailed)
            {
                OnReconnectedAfterConnectionFailure?.Invoke(this, null);
                _connectionFailed = false;
            }

            return true;
        }
    }

    public IModel CreateModel()
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException("No RabbitMQ connections are available to perform this action.");
        }

        return _connection.CreateModel();
    }

    public void PublishMessage(string message, string exchangeName, string routingKey)
    {
        using var channel = CreateModel();
        channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct, true);
        var body = Encoding.UTF8.GetBytes(message);
        channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: null, body: body);
    }

    public void StartConsuming(string queueName)
    {
        using var channel = CreateModel();
        channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (a, b) =>
        {
            var msgBody = b.Body.ToArray();
            var message = Encoding.UTF8.GetString(msgBody);
            await Task.CompletedTask;
            Console.WriteLine("Received message: {0}", message);
        };

        channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

        Console.WriteLine("Consuming messages...");
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        try
        {
            _connection.Dispose();
        }
        catch (IOException ex)
        {
            logger.LogCritical(ex.ToString());
        }
    }

    private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
    {
        _connectionFailed = true;
        logger.LogWarning("A RabbitMQ connection is shutdown. Trying to re-connect...");

        TryConnectIfNotDisposed();
    }

    private void OnCallbackException(object sender, CallbackExceptionEventArgs e)
    {
        _connectionFailed = true;
        logger.LogWarning("A RabbitMQ connection throw exception. Trying to re-connect...");

        TryConnectIfNotDisposed();
    }

    private void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
    {
        _connectionFailed = true;
        logger.LogWarning("A RabbitMQ connection is on shutdown. Trying to re-connect...");

        TryConnectIfNotDisposed();
    }

    private void OnConnectionUnblocked(object sender, EventArgs args)
    {
        _connectionFailed = true;
        logger.LogWarning("A RabbitMQ connection is unblocked. Trying to re-connect...");
        TryConnectIfNotDisposed();
    }

    private void TryConnectIfNotDisposed()
    {
        if (_disposed)
        {
            logger.LogInformation("RabbitMQ client is disposed. No action will be taken.");
            return;
        }

        TryConnect();
    }
}