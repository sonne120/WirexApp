using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WirexApp.Domain;
using WirexApp.Domain.User;

namespace WirexApp.Infrastructure.DataAccess
{
   public class InMemoryUserRepository : IUserRepository

    { 
        private readonly IDictionary<UserId, User> _user = new ConcurrentDictionary<UserId, User>();
       
        public void AddAsync(User user)
        {
            _user.Add(user._userId, user);
        }

        public Task<User> GetByIdAsync(UserId id)
        {
            return Task.FromResult(_user.Single(i => i.Key == id).Value);            
        }
    }
}
