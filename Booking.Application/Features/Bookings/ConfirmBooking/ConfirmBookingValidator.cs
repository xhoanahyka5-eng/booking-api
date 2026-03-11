using FluentValidation;

namespace Booking.Application.Features.Bookings.ConfirmBooking;

public class ConfirmBookingCommandValidator
    : AbstractValidator<ConfirmBookingCommand>
{
    public ConfirmBookingCommandValidator()
    {
        RuleFor(x => x.BookingId)
            .GreaterThan(0).WithMessage("BookingId must be greater than 0.");

        RuleFor(x => x.HostId)
            .NotEmpty().WithMessage("HostId is required.");
    }
}