using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WirexApp.Infrastructure.Outbox
{

    public interface IOutboxRepository
    {
        Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);

        Task<IEnumerable<OutboxMessage>> GetPendingMessagesAsync(int batchSize, CancellationToken cancellationToken = default);

        Task MarkAsProcessingAsync(Guid messageId, CancellationToken cancellationToken = default);

        Task MarkAsCompletedAsync(Guid messageId, CancellationToken cancellationToken = default);

        Task MarkAsFailedAsync(Guid messageId, string errorMessage, CancellationToken cancellationToken = default);

        Task<int> GetPendingCountAsync(CancellationToken cancellationToken = default);
    }
}
