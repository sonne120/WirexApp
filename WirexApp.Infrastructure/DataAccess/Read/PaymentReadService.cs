using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using WirexApp.Application.Payments.ReadModels;

namespace WirexApp.Infrastructure.DataAccess.Read
{
    /// <summary>
    /// Read service for Payment queries (Query side of CQRS)
    /// Uses in-memory cache for fast reads
    /// In production, this would read from a separate read database (e.g., MongoDB, Elasticsearch)
    /// </summary>
    public class PaymentReadService : IReadService<PaymentReadModel>
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<PaymentReadService> _logger;
        private readonly ConcurrentDictionary<Guid, PaymentReadModel> _readModels;

        private const string CacheKeyPrefix = "Payment_";
        private const int CacheExpirationMinutes = 10;

        public PaymentReadService(IMemoryCache cache, ILogger<PaymentReadService> logger)
        {
            _cache = cache;
            _logger = logger;
            _readModels = new ConcurrentDictionary<Guid, PaymentReadModel>();
        }

        public async Task<PaymentReadModel> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting payment read model {PaymentId}", id);

            // Try to get from cache first
            var cacheKey = $"{CacheKeyPrefix}{id}";
            if (_cache.TryGetValue(cacheKey, out PaymentReadModel cachedModel))
            {
                _logger.LogDebug("Payment read model {PaymentId} found in cache", id);
                return cachedModel;
            }

            // Get from in-memory storage (in production, this would be a read database query)
            if (_readModels.TryGetValue(id, out var model))
            {
                // Cache for future requests
                _cache.Set(cacheKey, model, TimeSpan.FromMinutes(CacheExpirationMinutes));
                return model;
            }

            _logger.LogWarning("Payment read model {PaymentId} not found", id);
            return null;
        }

        public async Task<IEnumerable<PaymentReadModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting all payment read models");

            await Task.CompletedTask;
            return _readModels.Values.ToList();
        }

        public async Task<IEnumerable<PaymentReadModel>> FindAsync(
            Expression<Func<PaymentReadModel, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Finding payment read models with predicate");

            await Task.CompletedTask;
            var compiledPredicate = predicate.Compile();
            return _readModels.Values.Where(compiledPredicate).ToList();
        }

        public async Task<PaymentReadModel> FirstOrDefaultAsync(
            Expression<Func<PaymentReadModel, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting first payment read model with predicate");

            await Task.CompletedTask;
            var compiledPredicate = predicate.Compile();
            return _readModels.Values.FirstOrDefault(compiledPredicate);
        }

        public async Task<int> CountAsync(
            Expression<Func<PaymentReadModel, bool>> predicate = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Counting payment read models");

            await Task.CompletedTask;

            if (predicate == null)
            {
                return _readModels.Count;
            }

            var compiledPredicate = predicate.Compile();
            return _readModels.Values.Count(compiledPredicate);
        }

        /// <summary>
        /// Internal method to update read model (called by projection event handlers)
        /// </summary>
        public void UpdateReadModel(PaymentReadModel readModel)
        {
            _logger.LogDebug("Updating payment read model {PaymentId}", readModel.PaymentId);

            _readModels.AddOrUpdate(readModel.PaymentId, readModel, (key, existing) => readModel);

            // Invalidate cache
            var cacheKey = $"{CacheKeyPrefix}{readModel.PaymentId}";
            _cache.Remove(cacheKey);
        }

        /// <summary>
        /// Internal method to remove read model
        /// </summary>
        public void RemoveReadModel(Guid paymentId)
        {
            _logger.LogDebug("Removing payment read model {PaymentId}", paymentId);

            _readModels.TryRemove(paymentId, out _);

            // Invalidate cache
            var cacheKey = $"{CacheKeyPrefix}{paymentId}";
            _cache.Remove(cacheKey);
        }
    }
}
