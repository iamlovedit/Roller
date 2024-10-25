namespace Roller.Infrastructure.EventBus.Subscriptions;

public class InMemoryEventBusSubscriptionManager : IEventBusSubscriptionManager
{
    private readonly Dictionary<string, List<Subscription>> _handlers = new();
    private readonly List<Type> _eventTypes = [];
    public event EventHandler<string> OnEventRemoved;
    public string GetEventIdentifier<TEvent>() => typeof(TEvent).Name;

    public Type GetEventTypeByName(string eventName) => _eventTypes.SingleOrDefault(t => t.Name == eventName);

    public IEnumerable<Subscription> GetHandlersForEvent(string eventName) => _handlers[eventName];


    public Dictionary<string, List<Subscription>> GetAllSubscriptions() => new(_handlers);


    public void AddSubscription<TEvent, TEventHandler>()
        where TEvent : IntegrationEvent
        where TEventHandler : IIntegrationEventHandler<TEvent>
    {
        var eventName = GetEventIdentifier<TEvent>();

        DoAddSubscription(typeof(TEvent), typeof(TEventHandler), eventName);

        if (!_eventTypes.Contains(typeof(TEvent)))
        {
            _eventTypes.Add(typeof(TEvent));
        }
    }

    public void RemoveSubscription<TEvent, TEventHandler>()
        where TEventHandler : IIntegrationEventHandler<TEvent>
        where TEvent : IntegrationEvent
    {
        var handlerToRemove = FindSubscriptionToRemove<TEvent, TEventHandler>();
        var eventName = GetEventIdentifier<TEvent>();
        DoRemoveHandler(eventName, handlerToRemove);
    }

    public void Clear()
    {
        _handlers.Clear();
        _eventTypes.Clear();
    }


    public bool IsEmpty => !_handlers.Keys.Any();
    public bool HasSubscriptionsForEvent(string eventName) => _handlers.ContainsKey(eventName);


    private void DoAddSubscription(Type eventType, Type handlerType, string eventName)
    {
        if (!HasSubscriptionsForEvent(eventName))
        {
            _handlers.Add(eventName, new List<Subscription>());
        }

        if (_handlers[eventName].Any(s => s.HandlerType == handlerType))
        {
            throw new ArgumentException($"Handler Type {handlerType.Name} already registered for '{eventName}'",
                nameof(handlerType));
        }

        _handlers[eventName].Add(new Subscription(eventType, handlerType));
    }

    private void DoRemoveHandler(string eventName, Subscription subscriptionToRemove)
    {
        if (subscriptionToRemove == null)
        {
            return;
        }

        _handlers[eventName].Remove(subscriptionToRemove);
        if (_handlers[eventName].Any())
        {
            return;
        }

        _handlers.Remove(eventName);
        var eventType = _eventTypes.SingleOrDefault(e => e.Name == eventName);
        if (eventType != null)
        {
            _eventTypes.Remove(eventType);
        }

        RaiseOnEventRemoved(eventName);
    }

    private void RaiseOnEventRemoved(string eventName)
    {
        var handler = OnEventRemoved;
        handler?.Invoke(this, eventName);
    }

    private Subscription FindSubscriptionToRemove<TEvent, TEventHandler>()
        where TEvent : IntegrationEvent
        where TEventHandler : IIntegrationEventHandler<TEvent>
    {
        var eventName = GetEventIdentifier<TEvent>();
        return DoFindSubscriptionToRemove(eventName, typeof(TEventHandler));
    }

    private Subscription DoFindSubscriptionToRemove(string eventName, Type handlerType)
    {
        return !HasSubscriptionsForEvent(eventName) ? null : _handlers[eventName].SingleOrDefault(s => s.HandlerType == handlerType);
    }
}