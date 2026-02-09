using System;
using System.Collections.Generic;
using System.Text;

namespace WirexApp.Domain.User
{
    class UserRegisteredEvent : DomainEventBase
    {
        public UserId userId { get; }   

        public UserRegisteredEvent(UserId userId)
        {
            this.userId = userId;
        }

    }
}
