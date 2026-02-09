using System;
using System.Threading.Tasks;
using WirexApp.Domain.User;

namespace WirexApp.Domain.BonusAccounts
{
    public interface IBonusAccountRepository
    {
        Task<BonusAccount> GetByIdAsync(BonusAccountId bonusAccountId);

        Task<BonusAccount> GetByUserIdAsync(UserId userId);

        Task AddAsync(BonusAccount bonusAccount);

        Task SaveAsync(BonusAccount bonusAccount);
    }
}
