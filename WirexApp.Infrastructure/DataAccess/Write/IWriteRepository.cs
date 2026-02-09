using System;
using System.Threading;
using System.Threading.Tasks;
using WirexApp.Domain;

namespace WirexApp.Infrastructure.DataAccess.Write
{
    public interface IWriteRepository<TAggregate> where TAggregate : IAggregateRoot
    {
        Task<TAggregate> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        void Add(TAggregate aggregate);

        void Update(TAggregate aggregate);

        void Delete(TAggregate aggregate);

        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
