using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WirexApp.Infrastructure.CDC.Events;
using WirexApp.Infrastructure.Messaging;

namespace WirexApp.Infrastructure.CDC
{
    public class CDCEventPublisher : ICDCEventPublisher
    {
        private readonly IMessageBus _messageBus;
        private readonly ILogger<CDCEventPublisher> _logger;

        // Kafka topic  cdc.{entity-type}
        private const string TopicPrefix = "cdc.";

        public CDCEventPublisher(IMessageBus messageBus, ILogger<CDCEventPublisher> logger)
        {
            _messageBus = messageBus;
            _logger = logger;
        }

        public async Task PublishAsync<TData>(CDCEvent<TData> cdcEvent, CancellationToken cancellationToken = default)
            where TData : class
        {
            var topic = GetTopicName(cdcEvent.EntityType);

            _logger.LogInformation(
                "Publishing CDC event: Entity={EntityType}, Id={EntityId}, Operation={Operation}, Topic={Topic}",
                cdcEvent.EntityType,
                cdcEvent.EntityId,
                cdcEvent.Operation,
                topic);

            try
            {
                await _messageBus.PublishAsync(topic, cdcEvent.EntityId, cdcEvent);

                _logger.LogInformation(
                    "CDC event published successfully: EventId={EventId}, Entity={EntityType}, Id={EntityId}",
                    cdcEvent.EventId,
                    cdcEvent.EntityType,
                    cdcEvent.EntityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to publish CDC event: EventId={EventId}, Entity={EntityType}, Id={EntityId}",
                    cdcEvent.EventId,
                    cdcEvent.EntityType,
                    cdcEvent.EntityId);
                throw;
            }
        }

        public async Task PublishCreateAsync<TData>(
            string entityType,
            string entityId,
            TData data,
            CancellationToken cancellationToken = default)
            where TData : class
        {
            var cdcEvent = CDCEvent<TData>.Create(entityType, entityId, data);
            await PublishAsync(cdcEvent, cancellationToken);
        }

        public async Task PublishUpdateAsync<TData>(
            string entityType,
            string entityId,
            TData data,
            CancellationToken cancellationToken = default)
            where TData : class
        {
            var cdcEvent = CDCEvent<TData>.Update(entityType, entityId, data, null, 0);
            await PublishAsync(cdcEvent, cancellationToken);
        }

        public async Task PublishDeleteAsync(
            string entityType,
            string entityId,
            CancellationToken cancellationToken = default)
        {
            var cdcEvent = CDCEvent<object>.Delete(entityType, entityId, 0);
            await PublishAsync(cdcEvent, cancellationToken);
        }

        private static string GetTopicName(string entityType)
        {
            return $"{TopicPrefix}{entityType.ToLowerInvariant()}";
        }
    }
}
