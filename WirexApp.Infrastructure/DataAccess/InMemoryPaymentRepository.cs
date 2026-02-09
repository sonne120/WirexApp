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


        public Payment GetByIdAsync(Guid id)
        {
            var events = _eventStore.GetEventsForAggregate(id);
            return new Payment(id, events);
        }

        public Payment GetByUserAccountAsync(UserAccount userAccount, DateTime dateTime)
        {
            throw new NotImplementedException();
        }

        public void Save(Payment payment, int expectedVersion)
        {
            //_eventStore.SaveEvents(payment.PaymentId, payment.GetUncommittedChanges(), expectedVersion);
            payment.MarkChangesAsCommitted();
        }

    }
}
