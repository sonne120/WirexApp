using System;
using System.Threading;
using System.Threading.Tasks;
using WirexApp.Domain.Payments;

namespace WirexApp.Application.Payments
{
    public class GetPaymentQueryHandler : IQueryHandler<GetPaymentQuery, PaymentDto>
    {
        private readonly IPaymentRepository _paymentRepository;

        public GetPaymentQueryHandler(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<PaymentDto> Handle(GetPaymentQuery query, CancellationToken cancellationToken)
        {
            var payment = await _paymentRepository.GetByIdAsync(query.PaymentId);

            if (payment == null)
            {
                return null;
            }
            
            return new PaymentDto
            {
                PaymentId = payment.PaymentId,
            };
        }
    }
}
