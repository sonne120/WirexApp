using System;

namespace WirexApp.Infrastructure.Messaging
{
    public interface IIntegrationEvent
    {
        Guid EventId { get; }

        DateTime OccurredOn { get; }

        string EventType { get; }
    }
}
