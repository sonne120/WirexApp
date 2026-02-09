using System;
using WirexApp.Domain.User;

namespace WirexApp.Domain.BonusAccounts
{
    public class BonusAccount : AggregateRoot
    {
        public BonusAccountId BonusAccountId { get; private set; }

        private UserId _userId;

        private decimal _bonusPoints;

        private DateTime _createdDate;

        private DateTime? _expiryDate;

        private bool _isActive;

        private BonusAccount()
        {
        }

        public BonusAccount(UserId userId, DateTime? expiryDate = null)
        {
            this.BonusAccountId = new BonusAccountId(Guid.NewGuid());
            this.Id = BonusAccountId.Value;
            this._userId = userId;
            this._bonusPoints = 0;
            this._createdDate = DateTime.UtcNow;
            this._expiryDate = expiryDate;
            this._isActive = true;
        }

        public void AddBonusPoints(decimal points)
        {
            if (points <= 0)
            {
                throw new ArgumentException("Bonus points must be positive", nameof(points));
            }

            if (!_isActive)
            {
                throw new InvalidOperationException("Bonus account is not active");
            }

            if (_expiryDate.HasValue && DateTime.UtcNow > _expiryDate.Value)
            {
                throw new InvalidOperationException("Bonus account has expired");
            }

            _bonusPoints += points;
        }

        public void RedeemBonusPoints(decimal points)
        {
            if (points <= 0)
            {
                throw new ArgumentException("Bonus points must be positive", nameof(points));
            }

            if (!_isActive)
            {
                throw new InvalidOperationException("Bonus account is not active");
            }

            if (_bonusPoints < points)
            {
                throw new InvalidOperationException("Insufficient bonus points");
            }

            _bonusPoints -= points;
        }

        public void Deactivate()
        {
            _isActive = false;
        }

        public void Activate()
        {
            _isActive = true;
        }

        public decimal GetBonusPoints() => _bonusPoints;

        public bool IsActive() => _isActive;

        public bool IsExpired() => _expiryDate.HasValue && DateTime.UtcNow > _expiryDate.Value;
    }
}
