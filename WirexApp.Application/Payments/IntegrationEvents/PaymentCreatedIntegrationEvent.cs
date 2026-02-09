using System;
using WirexApp.Infrastructure.Messaging;

namespace WirexApp.Application.Payments.IntegrationEvents
{
    public class PaymentCreatedIntegrationEvent : IntegrationEventBase
    {
        public Guid PaymentId { get; }

        public Guid UserId { get; }

        public decimal Amount { get; }

        public string Currency { get; }

        public PaymentCreatedIntegrationEvent(Guid paymentId, Guid userId, decimal amount, string currency)
        {
            PaymentId = paymentId;
            UserId = userId;
            Amount = amount;
            Currency = currency;
        }
    }
}
