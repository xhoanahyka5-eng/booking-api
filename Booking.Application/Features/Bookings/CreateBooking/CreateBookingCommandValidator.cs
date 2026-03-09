using FluentValidation;

namespace Booking.Application.Features.Bookings.CreateBooking;

public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
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
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required.")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.");
    }
}