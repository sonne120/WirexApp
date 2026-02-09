using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using WirexApp.Domain.User;
using WirexApp.Domain.UserAccounts;
using System.Linq;
using WirexApp.Domain;

namespace WirexApp.Infrastructure.DataAccess
{
    class InMemoryUserAccountRepository : IUserAccountRepository /// where T : DomainEventBase
    {

        private readonly IDictionary<UserId, UserAccount> _userAccount = new ConcurrentDictionary<UserId, UserAccount>();
        public  void Add(UserAccount userAccount)
        {
            _userAccount.Add(userAccount._userId, userAccount);
        }

        public Task<UserAccount> GetByIdAsync(UserId id)
        {
            return Task.FromResult(_userAccount.Single(i => i.Key == id).Value);
        }
    }
}
