using System.Threading;
using System.Threading.Tasks;
using WirexApp.Infrastructure.CDC.Events;

namespace WirexApp.Infrastructure.CDC
{
    public interface ICDCEventPublisher
    {
        Task PublishAsync<TData>(CDCEvent<TData> cdcEvent, CancellationToken cancellationToken = default)
            where TData : class;

        Task PublishCreateAsync<TData>(string entityType, string entityId, TData data, CancellationToken cancellationToken = default)
            where TData : class;

        Task PublishUpdateAsync<TData>(string entityType, string entityId, TData data, CancellationToken cancellationToken = default)
            where TData : class;

        Task PublishDeleteAsync(string entityType, string entityId, CancellationToken cancellationToken = default);
    }
}
