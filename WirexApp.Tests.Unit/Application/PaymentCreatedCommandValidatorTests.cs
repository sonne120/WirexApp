using FluentAssertions;
using WirexApp.Application.Payments;
using WirexApp.Domain;
using Xunit;

namespace WirexApp.Tests.Unit.Application
{
    public class PaymentCreatedCommandValidatorTests
    {
        private readonly PaymentCreatedCommandValidator _validator;

        public PaymentCreatedCommandValidatorTests()
        {
            _validator = new PaymentCreatedCommandValidator();
        }

        [Fact]
        public void Validate_ShouldPass_WhenCommandIsValid()
        {
            var command = new PaymentCreatedCommand(
                Guid.NewGuid(),
                Currency.USD,
                Currency.EUR,
                100m
            );

            var result = _validator.Validate(command);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ShouldFail_WhenUserIdIsEmpty()
        {
    
            var command = new PaymentCreatedCommand(
                Guid.Empty,
                Currency.USD,
                Currency.EUR,
                100m
            );

  
            var result = _validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "UserId");
        }

        [Fact]
        public void Validate_ShouldFail_WhenSourceValueIsNegative()
        {
            var command = new PaymentCreatedCommand(
                Guid.NewGuid(),
                Currency.USD,
                Currency.EUR,
                -10m
            );

            var result = _validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "SourceValue");
        }

        [Fact]
        public void Validate_ShouldFail_WhenSourceValueIsZero()
        {

            var command = new PaymentCreatedCommand(
                Guid.NewGuid(),
                Currency.USD,
                Currency.EUR,
                0m
            );
            var result = _validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "SourceValue");
        }

        [Fact]
        public void Validate_ShouldFail_WhenSourceAndTargetCurrenciesAreSame()
        {
            var command = new PaymentCreatedCommand(
                Guid.NewGuid(),
                Currency.USD,
                Currency.USD, 
                100m
            );

            var result = _validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => 
                e.ErrorMessage.Contains("same") || 
                e.ErrorMessage.Contains("different"));
        }

        [Fact]
        public void Validate_ShouldFail_WhenSourceValueIsTooLarge()
        {
            var command = new PaymentCreatedCommand(
                Guid.NewGuid(),
                Currency.USD,
                Currency.EUR,
                1000001m
            );

            var result = _validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "SourceValue");
        }

        [Fact]
        public void Validate_ShouldPass_WhenSourceValueIsAtTheLimit()
        {
            var command = new PaymentCreatedCommand(
                Guid.NewGuid(),
                Currency.USD,
                Currency.EUR,
                1000000m
            );

            var result = _validator.Validate(command);

            result.IsValid.Should().BeTrue();
        }
    }
}
