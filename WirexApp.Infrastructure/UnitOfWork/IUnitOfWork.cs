using System;
using System.Threading.Tasks;
using System.Threading;


namespace WirexApp.Infrastructure
{
    public interface IUnitOfWork
    {
        public Task<int> CommitAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
