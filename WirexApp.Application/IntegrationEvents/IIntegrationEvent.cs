using System;

namespace WirexApp.Application.IntegrationEvents
{
    public interface IIntegrationEvent
    {
        Guid EventId { get; }

        DateTime OccurredOn { get; }

        string EventType { get; }
    }
}
