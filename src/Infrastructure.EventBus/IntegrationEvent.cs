namespace Roller.Infrastructure.EventBus;

[method: JsonConstructor]
public class IntegrationEvent(Guid id, DateTime createdDate)
{
    public IntegrationEvent() : this(Guid.NewGuid(), DateTime.UtcNow)
    {
    }

    [JsonProperty] public Guid Id { get; private set; } = id;

    [JsonProperty] public DateTime CreatedDate { get; private set; } = createdDate;
}