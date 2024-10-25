using Roller.Infrastructure.EventBus;

namespace Roller.Tutorial;

public class MessageSentEvent : IntegrationEvent
{
    public string Message { get; set; }

    public override string ToString()
    {
        return $"ID: {Id} - Created at: {CreatedDate:MM/dd/yyyy} - Message: {Message}";
    }
}

public class MessageSentEventHandler(ILogger<MessageSentEvent> logger) : IIntegrationEventHandler<MessageSentEvent>
{
    public Task HandleAsync(MessageSentEvent message)
    {
        // Here you handle what happens when you receive an event of this type from the event bus.
        logger.LogInformation(message.ToString());
        return Task.CompletedTask;
    }
}