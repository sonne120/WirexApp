using System;
using Newtonsoft.Json;
using WirexApp.Domain.Payments;
using WirexApp.Application.Configuration;
using WirexApp.Domain;

namespace WirexApp.Application.Payments
{
    public class PaymentCreatedNotification<T> : DomainNotificationBase<PaymentPlacedEvent> 
    {
        public Guid _Id { get; }

        public PaymentCreatedNotification(PaymentPlacedEvent domainEvent) : base(domainEvent)
        {
            this._Id = domainEvent._Id;
        }
      
        public PaymentCreatedNotification(Guid Id) : base(null)
        {
            this._Id = Id;
        }
    }
}
