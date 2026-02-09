using System;
using WirexApp.Domain;

namespace WirexApp.Application.Payments
{
    public class PaymentCreatedCommand : ICommand
    {
        public Guid Id { get; }

        public Guid UserId { get; }

        public Currency SourceCurrency { get; }

        public Currency TargetCurrency { get; }

        public decimal SourceValue { get; }

        public PaymentCreatedCommand(Guid userId, Currency sourceCurrency, Currency targetCurrency, decimal sourceValue)
        {
            this.Id = Guid.NewGuid();
            this.UserId = userId;
            this.SourceCurrency = sourceCurrency;
            this.TargetCurrency = targetCurrency;
            this.SourceValue = sourceValue;
        }
    }
}
