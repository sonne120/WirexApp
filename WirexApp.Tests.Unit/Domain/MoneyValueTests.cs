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
            var amount = 100.50m;
            var currency = "USD";

            var moneyValue = MoneyValue.Of(amount, currency);

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
            var money1 = MoneyValue.Of(amount1, currency1);
            var money2 = MoneyValue.Of(amount2, currency2);

            var result = money1 + money2;

            result.Value.Should().Be(expected);
            result.Currency.Should().Be(currency1);
        }

        [Fact]
        public void OperatorPlus_ShouldThrowException_WhenCurrenciesDiffer()
        {
            var money1 = MoneyValue.Of(100, "USD");
            var money2 = MoneyValue.Of(100, "EUR");

            var act = () => money1 + money2;

            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData(5, 100, "USD", 500)]
        [InlineData(2, 50.50, "EUR", 101)]
        public void OperatorMultiply_ShouldMultiplyMoneyValue_ByInteger(
            int multiplier,
            decimal amount,
            string currency,
            decimal expected)
        {
            var money = MoneyValue.Of(amount, currency);

            var result = multiplier * money;

            result.Value.Should().Be(expected);
            result.Currency.Should().Be(currency);
        }

        [Theory]
        [InlineData(2.5, 100, "USD", 250)]
        [InlineData(0.5, 50, "EUR", 25)]
        public void OperatorMultiply_ShouldMultiplyMoneyValue_ByDecimal(
            decimal multiplier,
            decimal amount,
            string currency,
            decimal expected)
        {
            var money = MoneyValue.Of(amount, currency);

            var result = multiplier * money;

            result.Value.Should().Be(expected);
            result.Currency.Should().Be(currency);
        }

        [Fact]
        public void Equals_ShouldReturnTrue_ForSameValues()
        {
            var money1 = MoneyValue.Of(100, "USD");
            var money2 = MoneyValue.Of(100, "USD");


            money1.Equals(money2).Should().BeTrue();
        }

        [Fact]
        public void Of_ShouldCreateMoneyValue_FromAnotherMoneyValue()
        {
            var originalMoney = MoneyValue.Of(100, "USD");

            var newMoney = MoneyValue.Of(originalMoney);

            newMoney.Should().NotBeNull();
            newMoney.Value.Should().Be(originalMoney.Value);
            newMoney.Currency.Should().Be(originalMoney.Currency);
            newMoney.Should().NotBeSameAs(originalMoney); 
        }

        [Fact]
        public void Sum_ShouldCalculateTotal_ForListOfMoneyValues()
        {
            var moneyValues = new[]
            {
                MoneyValue.Of(100, "USD"),
                MoneyValue.Of(50, "USD"),
                MoneyValue.Of(25, "USD")
            };

            var sum = moneyValues.Sum();

            sum.Should().NotBeNull();
            sum.Value.Should().Be(175);
            sum.Currency.Should().Be("USD");
        }

        [Fact]
        public void Sum_ShouldCalculateTotal_ForListOfCustomObjects()
        {
    
            var items = new[]
            {
                new { Price = MoneyValue.Of(10, "EUR") },
                new { Price = MoneyValue.Of(20, "EUR") },
                new { Price = MoneyValue.Of(30, "EUR") }
            };


            var sum = items.Sum(x => x.Price);

            sum.Should().NotBeNull();
            sum.Value.Should().Be(60);
            sum.Currency.Should().Be("EUR");
        }

        [Fact]
        public void Equals_ShouldReturnFalse_ForDifferentValues()
        {
            var money1 = MoneyValue.Of(100, "USD");
            var money2 = MoneyValue.Of(200, "USD");

   
            money1.Equals(money2).Should().BeFalse();
        }
    }
}
