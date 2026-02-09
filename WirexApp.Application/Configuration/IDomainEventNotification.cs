
namespace WirexApp.Application.Configuration
{
    public interface IDomainEventNotification<out TEventType>
    {
        TEventType DomainEvent { get; }
    }
}
