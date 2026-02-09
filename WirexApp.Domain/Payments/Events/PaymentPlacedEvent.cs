using System;
using System.Collections.Generic;
using System.Text;
using WirexApp.Domain.UserAccounts;


namespace WirexApp.Domain.Payments
{
    public class PaymentPlacedEvent  : DomainEventBase  
    {
        public PaymentPlacedEvent(Guid Id, MoneyValue targetValue, UserAccount  userAccountGuid)
        {
            _Id = Id;
            _targetValue = targetValue;
            _userAccountGuid = userAccountGuid;
        }

        public Guid _Id { get; }
        public MoneyValue _targetValue { get; }
        public UserAccount _userAccountGuid { get; }
    }
}
