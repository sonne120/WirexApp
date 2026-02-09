using System;
using WirexApp.Domain.User;

namespace WirexApp.Domain.UserAccounts
{
    public class UserAccount : AggregateRoot
    {
        public UserAccountId UserAccountId { get; private set; }

        private UserId _userId;

        private MoneyValue _balance;

        private Currency _currency;

        private DateTime _createdDate;

        private bool _isActive;

        private UserAccount()
        {
        }

        public UserAccount(UserId userId, Currency currency)
        {
            this.UserAccountId = new UserAccountId(Guid.NewGuid());
            this.Id = UserAccountId.Value;
            this._userId = userId;
            this._currency = currency;
            this._balance = MoneyValue.Of(0, currency.ToString());
            this._createdDate = DateTime.UtcNow;
            this._isActive = true;
        }

        public void Deposit(MoneyValue amount)
        {
            if (amount.Currency != _currency.ToString())
            {
                throw new InvalidOperationException("Currency mismatch");
            }

            _balance = _balance + amount;
        }

        public void Withdraw(MoneyValue amount)
        {
            if (amount.Currency != _currency.ToString())
            {
                throw new InvalidOperationException("Currency mismatch");
            }

            if (_balance.Value < amount.Value)
            {
                throw new InvalidOperationException("Insufficient funds");
            }

            _balance = MoneyValue.Of(_balance.Value - amount.Value, _balance.Currency);
        }

        public void Deactivate()
        {
            _isActive = false;
        }

        public void Activate()
        {
            _isActive = true;
        }

        public MoneyValue GetBalance() => _balance;

        public bool IsActive() => _isActive;
    }
}
