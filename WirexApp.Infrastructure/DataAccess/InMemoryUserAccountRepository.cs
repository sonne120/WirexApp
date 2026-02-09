using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using WirexApp.Domain.User;
using WirexApp.Domain.UserAccounts;
using System.Linq;
using WirexApp.Domain;

namespace WirexApp.Infrastructure.DataAccess
{
    public class InMemoryUserAccountRepository : IUserAccountRepository
    {
        private readonly IMediator _bus;
        private readonly IEventStore _eventStore;
        private readonly IDictionary<Guid, Guid> _userIdToAccountId = new ConcurrentDictionary<Guid, Guid>();

        public InMemoryUserAccountRepository(IMediator bus, IEventStore eventStore)
        {
            this._eventStore = eventStore;
            this._bus = bus;
            this._eventStore.Bus = bus;
        }

        public Task<UserAccount?> GetByIdAsync(UserAccountId userAccountId)
        {
            try
            {
                var events = _eventStore.GetEventsForAggregate(userAccountId.Value);
                // UserAccount needs a constructor that takes Guid and events
                // For now, throw as not implemented
                throw new NotImplementedException("UserAccount event sourcing reconstruction not implemented");
            }
            catch (AggregateNotFoundException)
            {
                return Task.FromResult<UserAccount?>(null);
            }
        }

        public Task<UserAccount?> GetByUserIdAsync(UserId userId)
        {
            if (_userIdToAccountId.TryGetValue(userId.Value, out var accountId))
            {
                return GetByIdAsync(new UserAccountId(accountId));
            }
            return Task.FromResult<UserAccount?>(null);
        }

        public void Add(UserAccount userAccount)
        {
            _eventStore.SaveEvents(userAccount.UserAccountId.Value, userAccount.GetUncommittedChanges(), -1);
            userAccount.MarkChangesAsCommitted();
        }

        public Task SaveAsync(UserAccount userAccount)
        {
            _eventStore.SaveEvents(userAccount.UserAccountId.Value, userAccount.GetUncommittedChanges(), userAccount.Version);
            userAccount.MarkChangesAsCommitted();
            return Task.CompletedTask;
        }
    }
}
