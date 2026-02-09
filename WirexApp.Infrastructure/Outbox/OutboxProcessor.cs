using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WirexApp.Infrastructure.CDC.Events;
using WirexApp.Infrastructure.Messaging;

namespace WirexApp.Infrastructure.Outbox
{
    public class OutboxProcessor : BackgroundService
    {
        private readonly IOutboxRepository _outboxRepository;
        private readonly IMessageBus _messageBus;
        private readonly ILogger<OutboxProcessor> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);
        private const int BatchSize = 100;
        private const int MaxRetryCount = 3;

        public OutboxProcessor(
            IOutboxRepository outboxRepository,
            IMessageBus messageBus,
            ILogger<OutboxProcessor> logger)
        {
            _outboxRepository = outboxRepository;
            _messageBus = messageBus;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Outbox Processor started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOutboxMessagesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing outbox messages");
                }

                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("Outbox Processor stopped");
        }

        private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
        {
            var pendingMessages = await _outboxRepository.GetPendingMessagesAsync(BatchSize, cancellationToken);

            foreach (var message in pendingMessages)
            {
                if (message.RetryCount >= MaxRetryCount)
                {
                    _logger.LogWarning(
                        "Message {MessageId} exceeded max retry count, marking as failed",
                        message.Id);

                    await _outboxRepository.MarkAsFailedAsync(
                        message.Id,
                        "Max retry count exceeded",
                        cancellationToken);
                    continue;
                }

                try
                {
                    await _outboxRepository.MarkAsProcessingAsync(message.Id, cancellationToken);

                    _logger.LogDebug(
                        "Processing outbox message: Id={MessageId}, EntityType={EntityType}, Topic={Topic}",
                        message.Id,
                        message.EntityType,
                        message.Topic);

                    // Publish to Kafka
                    await _messageBus.PublishAsync(
                        message.Topic,
                        message.EntityId,
                        DeserializePayload(message.Payload));

                    await _outboxRepository.MarkAsCompletedAsync(message.Id, cancellationToken);

                    _logger.LogInformation(
                        "Outbox message published successfully: Id={MessageId}, EntityType={EntityType}",
                        message.Id,
                        message.EntityType);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error publishing outbox message: Id={MessageId}, EntityType={EntityType}",
                        message.Id,
                        message.EntityType);

                    await _outboxRepository.MarkAsFailedAsync(
                        message.Id,
                        ex.Message,
                        cancellationToken);
                }
            }

            var pendingCount = await _outboxRepository.GetPendingCountAsync(cancellationToken);
            if (pendingCount > 0)
            {
                _logger.LogDebug("Outbox pending messages: {Count}", pendingCount);
            }
        }

        private object DeserializePayload(string payload)
        {
            try
            {
                return JsonConvert.DeserializeObject(payload, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing outbox payload");
                throw;
            }
        }
    }
}
