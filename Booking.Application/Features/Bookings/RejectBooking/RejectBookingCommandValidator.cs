using FluentValidation;

namespace Booking.Application.Features.Bookings.RejectBooking;

public class RejectBookingCommandValidator
    : AbstractValidator<RejectBookingCommand>
{
    public RejectBookingCommandValidator()
    {
        RuleFor(x => x.BookingId)
            .GreaterThan(0).WithMessage("BookingId must be greater than 0.");

        RuleFor(x => x.HostId)
            .NotEmpty().WithMessage("HostId is required.");
    }
}