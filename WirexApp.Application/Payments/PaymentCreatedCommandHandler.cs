using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WirexApp.Domain;
using WirexApp.Domain.ExchangeRate;
using WirexApp.Domain.Payments;
using WirexApp.Domain.User;
using WirexApp.Domain.UserAccounts;

namespace WirexApp.Application.Payments
{
    public class PaymentCreatedCommandHandler : ICommandHandler<PaymentCreatedCommand>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICurrencyExchange _currencyExchange;

        public PaymentCreatedCommandHandler(
            IPaymentRepository paymentRepository,
            IUserAccountRepository userAccountRepository,
            IUserRepository userRepository,
            ICurrencyExchange currencyExchange)
        {
            _paymentRepository = paymentRepository;
            _userAccountRepository = userAccountRepository;
            _userRepository = userRepository;
            _currencyExchange = currencyExchange;
        }

        public async Task<Unit> Handle(PaymentCreatedCommand command, CancellationToken cancellationToken)
        {
            var userId = new UserId(command.UserId);
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {command.UserId} not found");
            }

            var userAccount = await _userAccountRepository.GetByUserIdAsync(userId);

            if (userAccount == null)
            {
                throw new InvalidOperationException($"User account for user {command.UserId} not found");
            }

            var sourceValue = MoneyValue.Of(command.SourceValue, command.SourceCurrency.ToString());

            var conversionRates = _currencyExchange.GetConversionRates(command.SourceCurrency)
                .ToList();

            var payment = new Payment(
                sourceValue,
                command.SourceCurrency,
                command.TargetCurrency,
                userAccount,
                conversionRates
            );

            payment.CreatePayment(sourceValue);

            _paymentRepository.Save(payment);

            return Unit.Value;
        }
    }
}
