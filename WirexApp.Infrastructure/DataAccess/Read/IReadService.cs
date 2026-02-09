using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace WirexApp.Infrastructure.DataAccess.Read
{
    public interface IReadService<TReadModel> where TReadModel : class
    {
        Task<TReadModel> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<IEnumerable<TReadModel>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<IEnumerable<TReadModel>> FindAsync(
            Expression<Func<TReadModel, bool>> predicate,
            CancellationToken cancellationToken = default);

        Task<TReadModel> FirstOrDefaultAsync(
            Expression<Func<TReadModel, bool>> predicate,
            CancellationToken cancellationToken = default);

        Task<int> CountAsync(
            Expression<Func<TReadModel, bool>> predicate = null,
            CancellationToken cancellationToken = default);
    }
}
