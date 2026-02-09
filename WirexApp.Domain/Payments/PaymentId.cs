using System;
using System.Collections.Generic;
using System.Text;

namespace WirexApp.Domain.Payments
{
    public class PaymentId : TypedIdValueBase
    {
        public PaymentId(Guid value) : base(value)
        {
        }
    }
}
