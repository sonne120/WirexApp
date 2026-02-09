using System;

namespace WirexApp.Domain.UserAccounts
{
    public class UserAccountId : TypedIdValueBase
    {
        public UserAccountId(Guid value) : base(value)
        {
        }
    }
}
