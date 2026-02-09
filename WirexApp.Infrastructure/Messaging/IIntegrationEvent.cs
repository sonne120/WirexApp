// This file is kept for backward compatibility
// Use WirexApp.Application.IntegrationEvents.IIntegrationEvent instead

namespace WirexApp.Infrastructure.Messaging
{
    [System.Obsolete("Use WirexApp.Application.IntegrationEvents.IIntegrationEvent instead")]
    public interface IIntegrationEvent : Application.IntegrationEvents.IIntegrationEvent
    {
    }
}
