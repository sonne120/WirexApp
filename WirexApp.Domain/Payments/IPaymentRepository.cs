using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WirexApp.Domain.UserAccounts;

namespace WirexApp.Domain.Payments
{
    public interface IPaymentRepository 
    {
        Payment GetByIdAsync(Guid id);

        Payment GetByUserAccountAsync(UserAccount userAccount, DateTime dateTime);

        void Save(Payment payment, int expectedVersion);
    }
}
