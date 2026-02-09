using System;
using System.Collections.Generic;
using System.Text;

namespace WirexApp.Domain.Payments.Events
{
    class PaymentRemoveEvent : DomainEventBase
    {
        public PaymentRemoveEvent(Guid Id)
        {
            this._Id = Id;
        }
        public Guid _Id { get; }
    }
}
