using System;
using System.Collections.Generic;
using System.Linq;
using WirexApp.Domain.ExchangeRate;
using WirexApp.Domain.Payments.Events;
using WirexApp.Domain.User;
using WirexApp.Domain.UserAccounts;

namespace WirexApp.Domain.Payments
{
    public class Payment : AggregateRoot  
    {
        public Guid PaymentId { get; private set; }

        private UserAccount _userAccount;

        private Currency _sourceCurrency;

        private Currency _targetCurrency;

        private MoneyValue _sourceValue;

        private MoneyValue _targetValue;

        private DateTime _createDate;

        private PaymentStatus _status;

        private bool _isRemoved;

        private bool _isEmailNotification;
        public Payment()
        {

        }
        public Payment(Guid id, IEnumerable<IDomainEvent> events)
        {
            this.PaymentId = id;
            LoadsFromHistory(events);
        }
        public Payment(MoneyValue sourceValue, Currency sourceCurrency, Currency targerCurrency, UserAccount _userAccount, List<ConversionRate> conversionRates)
        {
            this.PaymentId = Guid.NewGuid();
            this._userAccount = _userAccount;
            this._createDate = DateTime.UtcNow;
            this._sourceCurrency = sourceCurrency;
            this._targetCurrency = targerCurrency;
            this._sourceValue = sourceValue;
            this._status = PaymentStatus.ToPay;
            this._isRemoved = false;
            this._isEmailNotification = false;
            this.CalculateValue(this._sourceValue, targerCurrency, conversionRates);
        }

        public void CreatePayment(MoneyValue sourceValue)
        {            
           // this.AddDomainEvent(new PaymentPlacedEvent(this.PaymentId, this._targetValue, this._userAccount));
        }

        public void RemovePayment()
        {
            this._isRemoved = true;
            this.AddDomainEvent(new PaymentRemoveEvent(PaymentId));
        }

        private void CalculateValue(MoneyValue _sourceValue, Currency targetCurrency, List<ConversionRate> conversionRates)
        {
            if (targetCurrency != this._sourceCurrency)
            {
                var conversionRate = conversionRates.Single(i => i.SourceCurrency == this._sourceCurrency && i.TargetCurrency == targetCurrency);
                this._targetValue = conversionRate.Convert(this._sourceValue);
            }
            else
            {
                this._targetValue = _sourceValue;
            }
        }

        public void EmailNotification()
        {
            this._isEmailNotification = true;
        }
    }
}
