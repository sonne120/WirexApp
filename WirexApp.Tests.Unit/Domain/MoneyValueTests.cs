using FluentAssertions;
using WirexApp.Domain;
using Xunit;

namespace WirexApp.Tests.Unit.Domain
{
    public class MoneyValueTests
    {
        [Fact]
        public void Of_ShouldCreateMoneyValue_WithValidValues()
        {
            // Arrange
            var amount = 100.50m;
            var currency = "USD";

            // Act
            var moneyValue = MoneyValue.Of(amount, currency);

            // Assert
            moneyValue.Should().NotBeNull();
            moneyValue.Value.Should().Be(amount);
            moneyValue.Currency.Should().Be(currency);
        }

        [Theory]
        [InlineData(100, "USD", 50, "USD", 150)]
        [InlineData(99.99, "EUR", 0.01, "EUR", 100)]
        public void OperatorPlus_ShouldAddTwoMoneyValues_WithSameCurrency(
            decimal amount1, string currency1,
            decimal amount2, string currency2,
            decimal expected)
        {
            // Arrange
            var money1 = MoneyValue.Of(amount1, currency1);
            var money2 = MoneyValue.Of(amount2, currency2);

            // Act
            var result = money1 + money2;

            // Assert
            result.Value.Should().Be(expected);
            result.Currency.Should().Be(currency1);
        }

        [Fact]
        public void OperatorPlus_ShouldThrowException_WhenCurrenciesDiffer()
        {
            // Arrange
            var money1 = MoneyValue.Of(100, "USD");
            var money2 = MoneyValue.Of(100, "EUR");

            // Act
            var act = () => money1 + money2;

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData(5, 100, "USD", 500)]
        [InlineData(2, 50.50, "EUR", 101)]
        public void OperatorMultiply_ShouldMultiplyMoneyValue_ByNumber(
            int multiplier,
            decimal amount,
            string currency,
            decimal expected)
        {
            // Arrange
            var money = MoneyValue.Of(amount, currency);

            // Act
            var result = multiplier * money;

            // Assert
            result.Value.Should().Be(expected);
            result.Currency.Should().Be(currency);
        }

        [Fact]
        public void Equals_ShouldReturnTrue_ForSameValues()
        {
            // Arrange
            var money1 = MoneyValue.Of(100, "USD");
            var money2 = MoneyValue.Of(100, "USD");

            // Act & Assert
            money1.Equals(money2).Should().BeTrue();
        }

        [Fact]
        public void Equals_ShouldReturnFalse_ForDifferentValues()
        {
            // Arrange
            var money1 = MoneyValue.Of(100, "USD");
            var money2 = MoneyValue.Of(200, "USD");

            // Act & Assert
            money1.Equals(money2).Should().BeFalse();
        }
    }
}
