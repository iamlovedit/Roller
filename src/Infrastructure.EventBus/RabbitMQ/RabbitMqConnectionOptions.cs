namespace Roller.Infrastructure.EventBus.RabbitMQ;

public class RabbitMqConnectionOptions
{
    public const string SectionName = "RabbitMQ";

    public const string USER = "RABBITMQ_USER";

    public const string PASSWORD = "RABBITMQ_PASSWORD";

    public const string HOST = "RABBITMQ_HOST";

    public required string Username { get; set; }

    public required string Password { get; set; }

    public required string HostName { get; set; }

    public required string ExchangeName { get; set; }

    public required string QueueName { get; set; }

    public int TimeoutBeforeReconnecting { get; set; }

    public bool DispatchConsumersAsync { get; set; } = true;
}