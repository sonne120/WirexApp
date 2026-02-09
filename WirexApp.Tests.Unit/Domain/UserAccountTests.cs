using FluentAssertions;
using WirexApp.Domain;
using WirexApp.Domain.User;
using WirexApp.Domain.UserAccounts;
using Xunit;

namespace WirexApp.Tests.Unit.Domain
{
    public class UserAccountTests
    {
        [Fact]
        public void Constructor_ShouldCreateUserAccount_WithValidData()
        {
            // Arrange
            var userId = new UserId(Guid.NewGuid());
            var currency = Currency.USD;

            // Act
            var userAccount = new UserAccount(userId, currency);

            // Assert
            userAccount.Should().NotBeNull();
            userAccount.GetBalance().Value.Should().Be(0);
            userAccount.IsActive().Should().BeTrue();
        }

        [Fact]
        public void Deposit_ShouldIncreaseBalance_WhenValidAmount()
        {
            // Arrange
            var userId = new UserId(Guid.NewGuid());
            var currency = Currency.USD;
            var userAccount = new UserAccount(userId, currency);
            var depositAmount = MoneyValue.Of(500, "USD");

            // Act
            userAccount.Deposit(depositAmount);

            // Assert
            userAccount.GetBalance().Value.Should().Be(500);
        }

        [Fact]
        public void Withdraw_ShouldDecreaseBalance_WhenSufficientFunds()
        {
            // Arrange
            var userId = new UserId(Guid.NewGuid());
            var currency = Currency.USD;
            var userAccount = new UserAccount(userId, currency);
            userAccount.Deposit(MoneyValue.Of(1000, "USD"));
            var withdrawAmount = MoneyValue.Of(300, "USD");

            // Act
            userAccount.Withdraw(withdrawAmount);

            // Assert
            userAccount.GetBalance().Value.Should().Be(700);
        }

        [Fact]
        public void Withdraw_ShouldThrowException_WhenInsufficientFunds()
        {
            // Arrange
            var userId = new UserId(Guid.NewGuid());
            var currency = Currency.USD;
            var userAccount = new UserAccount(userId, currency);
            userAccount.Deposit(MoneyValue.Of(100, "USD"));
            var withdrawAmount = MoneyValue.Of(500, "USD");

            // Act
            var act = () => userAccount.Withdraw(withdrawAmount);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Insufficient funds*");
        }

        [Fact]
        public void Deactivate_ShouldSetIsActiveToFalse()
        {
            // Arrange
            var userId = new UserId(Guid.NewGuid());
            var currency = Currency.USD;
            var userAccount = new UserAccount(userId, currency);

            // Act
            userAccount.Deactivate();

            // Assert
            userAccount.IsActive().Should().BeFalse();
        }

        [Fact]
        public void Activate_ShouldSetIsActiveToTrue_WhenDeactivated()
        {
            // Arrange
            var userId = new UserId(Guid.NewGuid());
            var currency = Currency.USD;
            var userAccount = new UserAccount(userId, currency);
            userAccount.Deactivate();

            // Act
            userAccount.Activate();

            // Assert
            userAccount.IsActive().Should().BeTrue();
        }
    }
}
