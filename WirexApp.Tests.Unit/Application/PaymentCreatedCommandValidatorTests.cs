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
            // Arrange
            var command = new PaymentCreatedCommand(
                Guid.NewGuid(),
                Currency.USD,
                Currency.EUR,
                100m
            );

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ShouldFail_WhenUserIdIsEmpty()
        {
            // Arrange
            var command = new PaymentCreatedCommand(
                Guid.Empty,
                Currency.USD,
                Currency.EUR,
                100m
            );

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "UserId");
        }

        [Fact]
        public void Validate_ShouldFail_WhenSourceValueIsNegative()
        {
            // Arrange
            var command = new PaymentCreatedCommand(
                Guid.NewGuid(),
                Currency.USD,
                Currency.EUR,
                -10m
            );

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "SourceValue");
        }

        [Fact]
        public void Validate_ShouldFail_WhenSourceValueIsZero()
        {
            // Arrange
            var command = new PaymentCreatedCommand(
                Guid.NewGuid(),
                Currency.USD,
                Currency.EUR,
                0m
            );

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "SourceValue");
        }

        [Fact]
        public void Validate_ShouldFail_WhenSourceAndTargetCurrenciesAreSame()
        {
            // Arrange
            var command = new PaymentCreatedCommand(
                Guid.NewGuid(),
                Currency.USD,
                Currency.USD, // Same as source
                100m
            );

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => 
                e.ErrorMessage.Contains("same") || 
                e.ErrorMessage.Contains("different"));
        }
    }
}
