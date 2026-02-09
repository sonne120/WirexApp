using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WirexApp.Domain.Payments;
using System.Linq;
using MediatR;
using WirexApp.Domain.UserAccounts;
using WirexApp.Domain;

namespace WirexApp.Infrastructure.DataAccess
{
    public class InMemoryPaymentRepository : IPaymentRepository
    {
        private readonly IMediator _bus;
        private readonly IEventStore _eventStore;

        public InMemoryPaymentRepository(IMediator bus, IEventStore eventStore)
        {
            this._eventStore = eventStore;
            this._bus = bus;
            this._eventStore.Bus = bus;
        }


        public Task<Payment?> GetByIdAsync(Guid id)
        {
            var events = _eventStore.GetEventsForAggregate(id);
            var payment = new Payment(id, events);
            return Task.FromResult<Payment?>(payment);
        }

        public Task<Payment?> GetByUserAccountAsync(UserAccount userAccount, DateTime dateTime)
        {
            return Task.FromResult<Payment?>(null);
        }

        public void Save(Payment payment, int expectedVersion = 0)
        {
            _eventStore.SaveEvents(payment.PaymentId, payment.GetUncommittedChanges(), expectedVersion);
            payment.MarkChangesAsCommitted();
        }

    }
}
