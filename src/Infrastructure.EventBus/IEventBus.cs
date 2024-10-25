namespace Roller.Infrastructure.EventBus;

public interface IEventBus
{
    void Publish(IntegrationEvent integrationEvent);

    void Subscribe<TEvent, TEventHandler>() where TEvent : IntegrationEvent where TEventHandler : IIntegrationEventHandler<TEvent>;

    void Unsubscribe<TEvent, TEventHandler>() where TEventHandler : IIntegrationEventHandler<TEvent> where TEvent : IntegrationEvent;
}