using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using WirexApp.Domain;
using WirexApp.Domain.User;

namespace WirexApp.Infrastructure.DataAccess
{
   public class InMemoryUserRepository : IUserRepository
    {
        private readonly IMediator _bus;
        private readonly IEventStore _eventStore;

        public InMemoryUserRepository(IMediator bus, IEventStore eventStore)
        {
            this._eventStore = eventStore;
            this._bus = bus;
            this._eventStore.Bus = bus;
        }

        public Task<User?> GetByIdAsync(UserId userId)
        {
            try
            {
                var events = _eventStore.GetEventsForAggregate(userId.Value);
                // User needs a constructor that takes Guid and events
                // For now, return null if not found
                return Task.FromResult<User?>(null);
            }
            catch (AggregateNotFoundException)
            {
                return Task.FromResult<User?>(null);
            }
        }

        public void Save(User user)
        {
            _eventStore.SaveEvents(user._userId.Value, user.GetUncommittedChanges(), user.Version);
            user.MarkChangesAsCommitted();
        }
    }
}
