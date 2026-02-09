using FluentAssertions;
using Moq;
using WirexApp.Application.Payments;
using WirexApp.Domain;
using WirexApp.Domain.ExchangeRate;
using WirexApp.Domain.Payments;
using WirexApp.Domain.User;
using WirexApp.Domain.UserAccounts;
using Xunit;

namespace WirexApp.Tests.Unit.Application
{
    public class PaymentCreatedCommandHandlerTests
    {
        private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
        private readonly Mock<IUserAccountRepository> _userAccountRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ICurrencyExchange> _currencyExchangeMock;
        private readonly PaymentCreatedCommandHandler _handler;

        public PaymentCreatedCommandHandlerTests()
        {
            _paymentRepositoryMock = new Mock<IPaymentRepository>();
            _userAccountRepositoryMock = new Mock<IUserAccountRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _currencyExchangeMock = new Mock<ICurrencyExchange>();

            _handler = new PaymentCreatedCommandHandler(
                _paymentRepositoryMock.Object,
                _userAccountRepositoryMock.Object,
                _userRepositoryMock.Object,
                _currencyExchangeMock.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenUserNotFound()
        {
            // Arrange
            var command = new PaymentCreatedCommand(
                Guid.NewGuid(),
                Currency.USD,
                Currency.EUR,
                100m
            );

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<UserId>()))
                .ReturnsAsync((User?)null);

            // Act
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenUserAccountNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new PaymentCreatedCommand(
                userId,
                Currency.USD,
                Currency.EUR,
                100m
            );

            var user = new User("Test", "User", "123 Main St", "test@example.com");

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<UserId>()))
                .ReturnsAsync(user);

            _userAccountRepositoryMock
                .Setup(x => x.GetByUserIdAsync(It.IsAny<UserId>()))
                .ReturnsAsync((UserAccount?)null);

            // Act
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_ShouldCreatePayment_WhenValidCommand()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new PaymentCreatedCommand(
                userId,
                Currency.USD,
                Currency.EUR,
                100m
            );

            var user = new User("Test", "User", "123 Main St", "test@example.com");
            var userAccount = new UserAccount(
                new UserId(userId),
                Currency.USD
            );

            var conversionRates = new List<ConversionRate>
            {
                new ConversionRate(Currency.USD, Currency.EUR, 0.85m)
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<UserId>()))
                .ReturnsAsync(user);

            _userAccountRepositoryMock
                .Setup(x => x.GetByUserIdAsync(It.IsAny<UserId>()))
                .ReturnsAsync(userAccount);

            _currencyExchangeMock
                .Setup(x => x.GetConversionRates())
                .Returns(conversionRates);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _paymentRepositoryMock.Verify(
                x => x.Save(It.IsAny<Payment>(), It.IsAny<int>()),
                Times.Once
            );
        }
    }
}
