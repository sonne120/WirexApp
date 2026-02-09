using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WirexApp.Domain;
using WirexApp.Domain.BonusAccounts;

namespace WirexApp.Infrastructure.DataAccess
{
    class InMemoryBonusAccountRepository : IBonusAccountRepository
     { 
        private readonly IDictionary<BonusAccountId, BonusAccount> _bonusAccount = new ConcurrentDictionary<BonusAccountId, BonusAccount>();
        public void AddAsync(BonusAccount bonusAccount)
        {
            _bonusAccount.Add(bonusAccount._Id, bonusAccount);
        }

        public Task<BonusAccount> GetByIdAsync(BonusAccountId id)
        {
            return Task.FromResult(_bonusAccount.Single(i => i.Key == id).Value);
        }
    }
}
