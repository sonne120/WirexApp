using System;

namespace WirexApp.Application.IntegrationEvents
{
    public abstract class IntegrationEventBase : IIntegrationEvent
    {
        public Guid EventId { get; }

        public DateTime OccurredOn { get; }

        public string EventType { get; }

        protected IntegrationEventBase()
        {
            EventId = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
            EventType = GetType().Name;
        }

        protected IntegrationEventBase(Guid eventId, DateTime occurredOn)
        {
            EventId = eventId;
            OccurredOn = occurredOn;
            EventType = GetType().Name;
        }
    }
}
