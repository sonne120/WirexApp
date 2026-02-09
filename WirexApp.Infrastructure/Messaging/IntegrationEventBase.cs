// This file is kept for backward compatibility
// Use WirexApp.Application.IntegrationEvents.IntegrationEventBase instead

using WirexApp.Application.IntegrationEvents;

namespace WirexApp.Infrastructure.Messaging
{
    [System.Obsolete("Use WirexApp.Application.IntegrationEvents.IntegrationEventBase instead")]
    public abstract class IntegrationEventBase : Application.IntegrationEvents.IntegrationEventBase
    {
    }
}
