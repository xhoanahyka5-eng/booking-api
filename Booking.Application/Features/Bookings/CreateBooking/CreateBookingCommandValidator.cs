using FluentValidation;

namespace Booking.Application.Features.Bookings.CreateBooking;

public class CreateBookingCommandValidator
    : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        RuleFor(x => x.GuestId)
            .NotEmpty().WithMessage("GuestId is required.");

        RuleFor(x => x.PropertyId)
            .GreaterThan(0).WithMessage("PropertyId must be greater than 0.");

        RuleFor(x => x.GuestCount)
            .GreaterThan(0).WithMessage("GuestCount must be greater than 0.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("StartDate is required.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("EndDate is required.");

        RuleFor(x => x)
            .Must(x => x.EndDate > x.StartDate)
            .WithMessage("EndDate must be after StartDate.");
    }
}