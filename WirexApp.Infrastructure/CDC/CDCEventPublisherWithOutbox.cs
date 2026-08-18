using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WirexApp.Infrastructure.CDC.Events;
using WirexApp.Infrastructure.Outbox;

namespace WirexApp.Infrastructure.CDC
{
    public class CDCEventPublisherWithOutbox : ICDCEventPublisher
    {
        private readonly IOutboxRepository _outboxRepository;
        private readonly ILogger<CDCEventPublisherWithOutbox> _logger;
        private readonly CDCConfiguration _cdcConfiguration;

        public CDCEventPublisherWithOutbox(
            IOutboxRepository outboxRepository,
            IOptions<CDCConfiguration> cdcConfigurationOptions,
            ILogger<CDCEventPublisherWithOutbox> logger)
        {
            _outboxRepository = outboxRepository;
            _logger = logger;
            _cdcConfiguration = cdcConfigurationOptions.Value;
        }

        public async Task PublishAsync<TData>(CDCEvent<TData> cdcEvent, CancellationToken cancellationToken = default)
            where TData : class
        {
            var topic = GetTopicName(cdcEvent.EntityType);

            _logger.LogInformation(
                "Saving CDC event to Outbox: Entity={EntityType}, Id={EntityId}, Operation={Operation}",
                cdcEvent.EntityType,
                cdcEvent.EntityId,
                cdcEvent.Operation);

            try
            {
                var payload = JsonConvert.SerializeObject(cdcEvent, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                });
                
                var outboxMessage = new OutboxMessage
                {
                    EntityType = cdcEvent.EntityType,
                    EntityId = cdcEvent.EntityId,
                    EventType = cdcEvent.GetType().Name,
                    Payload = payload,
                    Topic = topic
                };

                await _outboxRepository.AddAsync(outboxMessage, cancellationToken);

                _logger.LogInformation(
                    "CDC event saved to Outbox: EventId={EventId}, OutboxId={OutboxId}",
                    cdcEvent.EventId,
                    outboxMessage.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to save CDC event to Outbox: EventId={EventId}",
                    cdcEvent.EventId);
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

        private string GetTopicName(string entityType)
        {
            if (_cdcConfiguration.TopicMappings.TryGetValue(entityType, out var topic) && !string.IsNullOrEmpty(topic))
            {
                return topic;
            }

            _logger.LogWarning("No topic mapping found for entity type {EntityType} in CDCConfiguration. Using default naming convention 'cdc.{{entityType}}'.", entityType);
            const string TopicPrefix = "cdc.";
            return $"{TopicPrefix}{entityType.ToLowerInvariant()}";
        }
    }
}
