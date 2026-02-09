using System;
using System.Threading.Tasks;
using WirexApp.Domain.User;

namespace WirexApp.Domain.UserAccounts
{
    public interface IUserAccountRepository
    {
        Task<UserAccount?> GetByIdAsync(UserAccountId userAccountId);

        Task<UserAccount?> GetByUserIdAsync(UserId userId);

        void Add(UserAccount userAccount);

        Task SaveAsync(UserAccount userAccount);
    }
}
