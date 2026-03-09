using FluentValidation;

namespace Booking.Application.Features.Properties.SetAvailability;

public class SetAvailabilityCommandValidator : AbstractValidator<SetAvailabilityCommand>
{
    public SetAvailabilityCommandValidator()
    {
        RuleFor(x => x.PropertyId)
            .GreaterThan(0).WithMessage("PropertyId must be greater than 0.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0 when property is available.")
            .When(x => x.IsAvailable);
    }
}