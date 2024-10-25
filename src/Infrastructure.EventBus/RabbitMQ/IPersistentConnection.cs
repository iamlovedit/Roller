namespace Roller.Infrastructure.EventBus.RabbitMQ;

public interface IPersistentConnection
{
    event EventHandler OnReconnectedAfterConnectionFailure;
    bool IsConnected { get; }

    bool TryConnect();

    IModel CreateModel();

    void PublishMessage(string message, string exchangeName, string routingKey);

    void StartConsuming(string queueName);
}