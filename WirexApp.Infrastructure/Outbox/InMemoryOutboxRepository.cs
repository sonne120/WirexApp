using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WirexApp.Infrastructure.Outbox
{

    public class InMemoryOutboxRepository : IOutboxRepository
    {
        private readonly ConcurrentDictionary<Guid, OutboxMessage> _messages;

        public InMemoryOutboxRepository()
        {
            _messages = new ConcurrentDictionary<Guid, OutboxMessage>();
        }

        public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            _messages.TryAdd(message.Id, message);
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<OutboxMessage>> GetPendingMessagesAsync(
            int batchSize,
            CancellationToken cancellationToken = default)
        {
            var pending = _messages.Values
                .Where(m => m.Status == OutboxMessageStatus.Pending)
                .OrderBy(m => m.CreatedAt)
                .Take(batchSize)
                .ToList();

            return await Task.FromResult(pending);
        }

        public async Task MarkAsProcessingAsync(Guid messageId, CancellationToken cancellationToken = default)
        {
            if (_messages.TryGetValue(messageId, out var message))
            {
                message.Status = OutboxMessageStatus.Processing;
            }
            await Task.CompletedTask;
        }

        public async Task MarkAsCompletedAsync(Guid messageId, CancellationToken cancellationToken = default)
        {
            if (_messages.TryGetValue(messageId, out var message))
            {
                message.Status = OutboxMessageStatus.Completed;
                message.ProcessedAt = DateTime.UtcNow;
            }
            await Task.CompletedTask;
        }

        public async Task MarkAsFailedAsync(
            Guid messageId,
            string errorMessage,
            CancellationToken cancellationToken = default)
        {
            if (_messages.TryGetValue(messageId, out var message))
            {
                message.Status = OutboxMessageStatus.Failed;
                message.RetryCount++;
                message.ErrorMessage = errorMessage;
            }
            await Task.CompletedTask;
        }

        public async Task<int> GetPendingCountAsync(CancellationToken cancellationToken = default)
        {
            var count = _messages.Values.Count(m => m.Status == OutboxMessageStatus.Pending);
            return await Task.FromResult(count);
        }
    }
}
