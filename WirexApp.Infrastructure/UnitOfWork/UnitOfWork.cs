using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace WirexApp.Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ILogger<UnitOfWork> _logger;
        private bool _disposed;

        public UnitOfWork(ILogger<UnitOfWork> logger)
        {
            _logger = logger;
        }

        public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Committing transaction");

                // In a real implementation with EF Core or Dapper, you would:
                // 1. Save all changes to the database
                // 2. Commit the transaction
                // For now with in-memory implementation, we just return success

                await Task.CompletedTask;

                _logger.LogInformation("Transaction committed successfully");

                return 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error committing transaction");
                throw;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _logger.LogDebug("Disposing Unit of Work");
                }

                _disposed = true;
            }
        }
    }
}
