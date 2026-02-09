using System;

namespace WirexApp.Application.Payments.IntegrationEvents
{
    public class PaymentCreatedIntegrationEvent : Application.IntegrationEvents.IntegrationEventBase
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
