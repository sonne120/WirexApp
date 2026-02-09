using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using WirexApp.Domain;
using WirexApp.Domain.BonusAccounts;
using WirexApp.Domain.User;

namespace WirexApp.Infrastructure.DataAccess
{
    public class InMemoryBonusAccountRepository : IBonusAccountRepository
    {
        private readonly IMediator _bus;
        private readonly IEventStore _eventStore;
        private readonly IDictionary<Guid, Guid> _userIdToBonusAccountId = new ConcurrentDictionary<Guid, Guid>();

        public InMemoryBonusAccountRepository(IMediator bus, IEventStore eventStore)
        {
            this._eventStore = eventStore;
            this._bus = bus;
            this._eventStore.Bus = bus;
        }

        public Task<BonusAccount> GetByIdAsync(BonusAccountId bonusAccountId)
        {
            try
            {
                var events = _eventStore.GetEventsForAggregate(bonusAccountId.Value);

                throw new NotImplementedException("BonusAccount event sourcing reconstruction not implemented");
            }
            catch (AggregateNotFoundException)
            {
                throw new InvalidOperationException($"BonusAccount with id {bonusAccountId.Value} not found");
            }
        }

        public Task<BonusAccount> GetByUserIdAsync(UserId userId)
        {
            if (_userIdToBonusAccountId.TryGetValue(userId.Value, out var bonusAccountId))
            {
                return GetByIdAsync(new BonusAccountId(bonusAccountId));
            }
            throw new InvalidOperationException($"BonusAccount for user {userId.Value} not found");
        }

        public Task AddAsync(BonusAccount bonusAccount)
        {
            _eventStore.SaveEvents(bonusAccount.BonusAccountId.Value, bonusAccount.GetUncommittedChanges(), -1);
            bonusAccount.MarkChangesAsCommitted();
            return Task.CompletedTask;
        }

        public Task SaveAsync(BonusAccount bonusAccount)
        {
            _eventStore.SaveEvents(bonusAccount.BonusAccountId.Value, bonusAccount.GetUncommittedChanges(), bonusAccount.Version);
            bonusAccount.MarkChangesAsCommitted();
            return Task.CompletedTask;
        }
    }
}
