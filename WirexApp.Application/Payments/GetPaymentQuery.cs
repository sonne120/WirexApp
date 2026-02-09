using System;

namespace WirexApp.Application.Payments
{
    public class GetPaymentQuery : IQuery<PaymentDto>
    {
        public Guid Id { get; }

        public Guid PaymentId { get; }

        public GetPaymentQuery(Guid paymentId)
        {
            this.Id = Guid.NewGuid();
            this.PaymentId = paymentId;
        }
    }
}
