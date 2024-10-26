namespace Roller.Infrastructure.EventBus;

public class IntegrationEvent(Guid id, DateTime createdDate)
{
    public IntegrationEvent() : this(Guid.NewGuid(), DateTime.UtcNow)
    {
    }

    public Guid Id { get; private set; } = id;

    public DateTime CreatedDate { get; private set; } = createdDate;
}