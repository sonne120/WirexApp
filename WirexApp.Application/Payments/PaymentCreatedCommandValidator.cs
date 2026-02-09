using FluentValidation;
using System;

namespace WirexApp.Application.Payments
{
    public class PaymentCreatedCommandValidator : AbstractValidator<PaymentCreatedCommand>
    {
        public PaymentCreatedCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required");

            RuleFor(x => x.SourceValue)
                .GreaterThan(0)
                .WithMessage("Source value must be greater than zero")
                .LessThanOrEqualTo(1000000)
                .WithMessage("Source value cannot exceed 1,000,000");

            RuleFor(x => x.SourceCurrency)
                .IsInEnum()
                .WithMessage("Invalid source currency");

            RuleFor(x => x.TargetCurrency)
                .IsInEnum()
                .WithMessage("Invalid target currency");

            RuleFor(x => x)
                .Must(x => x.SourceCurrency != x.TargetCurrency)
                .WithMessage("Source and target currencies must be different");
        }
    }
}
