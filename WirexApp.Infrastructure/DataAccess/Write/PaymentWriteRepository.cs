using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using WirexApp.Domain.Payments;
using WirexApp.Infrastructure.CDC;
using WirexApp.Infrastructure.CDC.Models;
using WirexApp.Infrastructure.Messaging;

namespace WirexApp.Infrastructure.DataAccess.Write
{
    /// <summary>
    /// Write-side repository for Payment aggregate (Command side)
    /// Responsible for persisting domain events and publishing CDC events to Kafka
    /// </summary>
    public class PaymentWriteRepository : IWriteRepository<Payment>
    {
        private readonly IEventStore _eventStore;
        private readonly IMediator _mediator;
        private readonly IMessageBus _messageBus;
        private readonly ICDCEventPublisher _cdcPublisher;
        private readonly ILogger<PaymentWriteRepository> _logger;
        private readonly ConcurrentDictionary<Guid, Payment> _inMemoryCache;

        public PaymentWriteRepository(
            IEventStore eventStore,
            IMediator mediator,
            IMessageBus messageBus,
            ICDCEventPublisher cdcPublisher,
            ILogger<PaymentWriteRepository> logger)
        {
            _eventStore = eventStore;
            _mediator = mediator;
            _messageBus = messageBus;
            _cdcPublisher = cdcPublisher;
            _logger = logger;
            _inMemoryCache = new ConcurrentDictionary<Guid, Payment>();
        }

        public async Task<Payment> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Loading payment aggregate {PaymentId} from event store", id);

            var events = await _eventStore.GetEventsForAggregate(id);

            if (!events.Any())
            {
                _logger.LogWarning("Payment {PaymentId} not found in event store", id);
                return null;
            }

            var payment = new Payment(id, events);

            _logger.LogDebug("Payment aggregate {PaymentId} loaded with {EventCount} events", id, events.Count());

            return payment;
        }

        public void Add(Payment aggregate)
        {
            _logger.LogDebug("Adding payment aggregate {PaymentId}", aggregate.PaymentId);
            _inMemoryCache.TryAdd(aggregate.PaymentId, aggregate);
        }

        public void Update(Payment aggregate)
        {
            _logger.LogDebug("Updating payment aggregate {PaymentId}", aggregate.PaymentId);
            _inMemoryCache.AddOrUpdate(aggregate.PaymentId, aggregate, (key, existing) => aggregate);
        }

        public void Delete(Payment aggregate)
        {
            _logger.LogDebug("Deleting payment aggregate {PaymentId}", aggregate.PaymentId);
            _inMemoryCache.TryRemove(aggregate.PaymentId, out _);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Saving changes for {Count} payment aggregates", _inMemoryCache.Count);

            foreach (var payment in _inMemoryCache.Values)
            {
                if (payment.DomainEvents.Any())
                {
                    _logger.LogDebug("Saving {EventCount} events for payment {PaymentId}",
                        payment.DomainEvents.Count, payment.PaymentId);

                    // Save domain events to event store
                    await _eventStore.SaveEvents(
                        payment.PaymentId,
                        payment.DomainEvents,
                        payment.Version);

                    // Publish domain events via MediatR
                    foreach (var domainEvent in payment.DomainEvents)
                    {
                        await _mediator.Publish(domainEvent, cancellationToken);
                    }

                    // Publish integration events to Kafka
                    // This could be done via a domain event handler instead
                    await PublishIntegrationEventsAsync(payment, cancellationToken);

                    payment.MarkChangesAsCommitted();
                }
            }

            _inMemoryCache.Clear();
            _logger.LogInformation("Changes saved successfully");
        }

        private async Task PublishIntegrationEventsAsync(Payment payment, CancellationToken cancellationToken)
        {
            // 1. Publish domain events as integration events
            foreach (var domainEvent in payment.DomainEvents)
            {
                _logger.LogDebug("Publishing integration event for domain event {EventType}",
                    domainEvent.GetType().Name);

                await _messageBus.PublishAsync(
                    "payment-events",
                    payment.PaymentId.ToString(),
                    domainEvent);
            }

            // 2. Publish CDC event for read-side synchronization
            var cdcData = MapPaymentToCDCData(payment);

            _logger.LogInformation("Publishing CDC event for payment {PaymentId}", payment.PaymentId);

            await _cdcPublisher.PublishCreateAsync(
                "Payment",
                payment.PaymentId.ToString(),
                cdcData,
                cancellationToken);
        }

        private PaymentCDCData MapPaymentToCDCData(Payment payment)
        {
            // Map Payment aggregate to CDC data model
            // In production, you'd expose necessary properties from Payment aggregate
            return new PaymentCDCData
            {
                PaymentId = payment.PaymentId,
                // UserAccountId = payment.UserAccountId, // Would be exposed from aggregate
                // UserId = payment.UserId,
                // SourceCurrency = payment.SourceCurrency.ToString(),
                // TargetCurrency = payment.TargetCurrency.ToString(),
                // SourceValue = payment.SourceValue.Value,
                // TargetValue = payment.TargetValue.Value,
                Status = "ToPay",
                CreateDate = DateTime.UtcNow,
                IsRemoved = false,
                IsEmailNotificationSent = false,
                Version = payment.Version,
                CapturedAt = DateTime.UtcNow
            };
        }
    }
}
