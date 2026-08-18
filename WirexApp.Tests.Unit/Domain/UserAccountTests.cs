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
            var userId = new UserId(Guid.NewGuid());
            var currency = Currency.USD;

            var userAccount = new UserAccount(userId, currency);

            userAccount.Should().NotBeNull();
            userAccount.GetBalance().Value.Should().Be(0);
            userAccount.IsActive().Should().BeTrue();
        }

        [Fact]
        public void Deposit_ShouldIncreaseBalance_WhenValidAmount()
        {
            var userId = new UserId(Guid.NewGuid());
            var currency = Currency.USD;
            var userAccount = new UserAccount(userId, currency);
            var depositAmount = MoneyValue.Of(500, "USD");

            userAccount.Deposit(depositAmount);

            userAccount.GetBalance().Value.Should().Be(500);
        }

        [Fact]
        public void Deposit_ShouldDecreaseBalance_WhenDepositingNegativeAmount()
        {
            var userId = new UserId(Guid.NewGuid());
            var currency = Currency.USD;
            var userAccount = new UserAccount(userId, currency);
            userAccount.Deposit(MoneyValue.Of(500, "USD"));
            var negativeDeposit = MoneyValue.Of(-100, "USD");

            userAccount.Deposit(negativeDeposit);

            userAccount.GetBalance().Value.Should().Be(400);
        }

        [Fact]
        public void Deposit_ShouldThrowException_WhenCurrencyMismatch()
        {
            var userId = new UserId(Guid.NewGuid());
            var currency = Currency.USD;
            var userAccount = new UserAccount(userId, currency);
            var depositAmount = MoneyValue.Of(500, "EUR");

            var act = () => userAccount.Deposit(depositAmount);

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Currency mismatch*");
        }

        [Fact]
        public void Withdraw_ShouldDecreaseBalance_WhenSufficientFunds()
        {
            var userId = new UserId(Guid.NewGuid());
            var currency = Currency.USD;
            var userAccount = new UserAccount(userId, currency);
            userAccount.Deposit(MoneyValue.Of(1000, "USD"));
            var withdrawAmount = MoneyValue.Of(300, "USD");

            userAccount.Withdraw(withdrawAmount);

            userAccount.GetBalance().Value.Should().Be(700);
        }

        [Fact]
        public void Withdraw_ShouldIncreaseBalance_WhenWithdrawingNegativeAmount()
        {
    
            var userId = new UserId(Guid.NewGuid());
            var currency = Currency.USD;
            var userAccount = new UserAccount(userId, currency);
            userAccount.Deposit(MoneyValue.Of(200, "USD"));
            var negativeWithdrawal = MoneyValue.Of(-100, "USD");


            userAccount.Withdraw(negativeWithdrawal);

            userAccount.GetBalance().Value.Should().Be(300);
        }

        [Fact]
        public void Withdraw_ShouldThrowException_WhenCurrencyMismatch()
        {
            var userId = new UserId(Guid.NewGuid());
            var currency = Currency.USD;
            var userAccount = new UserAccount(userId, currency);
            userAccount.Deposit(MoneyValue.Of(1000, "USD"));
            var withdrawAmount = MoneyValue.Of(300, "EUR");


            var act = () => userAccount.Withdraw(withdrawAmount);

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Currency mismatch*");
        }

        [Fact]
        public void Withdraw_ShouldThrowException_WhenInsufficientFunds()
        {
            var userId = new UserId(Guid.NewGuid());
            var currency = Currency.USD;
            var userAccount = new UserAccount(userId, currency);
            userAccount.Deposit(MoneyValue.Of(100, "USD"));
            var withdrawAmount = MoneyValue.Of(500, "USD");

            var act = () => userAccount.Withdraw(withdrawAmount);

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Insufficient funds*");
        }

        [Fact]
        public void Deactivate_ShouldSetIsActiveToFalse()
        {
            var userId = new UserId(Guid.NewGuid());
            var currency = Currency.USD;
            var userAccount = new UserAccount(userId, currency);

            userAccount.Deactivate();

            userAccount.IsActive().Should().BeFalse();
        }

        [Fact]
        public void Deactivate_ShouldRemainInactive_WhenAlreadyDeactivated()
        {
            var userId = new UserId(Guid.NewGuid());
            var currency = Currency.USD;
            var userAccount = new UserAccount(userId, currency);
            userAccount.Deactivate();
            userAccount.Deactivate();

            userAccount.IsActive().Should().BeFalse();
        }

        [Fact]
        public void Activate_ShouldSetIsActiveToTrue_WhenDeactivated()
        {
            var userId = new UserId(Guid.NewGuid());
            var currency = Currency.USD;
            var userAccount = new UserAccount(userId, currency);
            userAccount.Deactivate();

            userAccount.Activate();


            userAccount.IsActive().Should().BeTrue();
        }
        [Fact]
        public void Activate_ShouldRemainActive_WhenAlreadyActive()
        {
            var userId = new UserId(Guid.NewGuid());
            var currency = Currency.USD;
            var userAccount = new UserAccount(userId, currency);


            userAccount.Activate();

            userAccount.IsActive().Should().BeTrue();
        }
    }
}
