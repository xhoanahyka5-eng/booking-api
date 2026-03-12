using FluentValidation;

namespace Booking.Application.Features.Bookings.GetMyBookings;

public class GetMyBookingsQueryValidator
    : AbstractValidator<GetMyBookingsQuery>
{
    public GetMyBookingsQueryValidator()
    {
        RuleFor(x => x.GuestId)
            .NotEmpty();

        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50);

        RuleFor(x => x.Scope)
            .Must(x => string.IsNullOrWhiteSpace(x) ||
                       x.Equals("upcoming", StringComparison.OrdinalIgnoreCase) ||
                       x.Equals("past", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Scope must be 'upcoming' or 'past'.");
    }
}