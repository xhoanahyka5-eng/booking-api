using FluentValidation;

namespace Booking.Application.Features.Properties.Users.BecomeHost;

public class BecomeHostCommandValidator : AbstractValidator<BecomeHostCommand>
{
    public BecomeHostCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.IdentityCardNumber)
            .NotEmpty().WithMessage("Identity card number is required.")
            .MaximumLength(50).WithMessage("Identity card number cannot exceed 50 characters.");

        RuleFor(x => x.BusinessName)
            .MaximumLength(100).WithMessage("Business name cannot exceed 100 characters.");
    }
}