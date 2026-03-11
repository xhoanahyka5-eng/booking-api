using FluentValidation;

namespace Booking.Application.Features.Bookings.CancelBooking;

public class CancelBookingCommandValidator
    : AbstractValidator<CancelBookingCommand>
{
    public CancelBookingCommandValidator()
    {
        RuleFor(x => x.BookingId)
            .GreaterThan(0).WithMessage("BookingId must be greater than 0.");

        RuleFor(x => x.GuestId)
            .NotEmpty().WithMessage("GuestId is required.");
    }
}