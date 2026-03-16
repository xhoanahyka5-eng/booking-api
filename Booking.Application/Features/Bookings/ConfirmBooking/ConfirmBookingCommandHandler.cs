using Booking.Application.Abstractions.Email;
using Booking.Application.Common.Exceptions;
using Booking.Application.Features.Bookings.Persistence;
using Booking.Application.Features.Users.Persistence;
using Booking.Domain.Entities.Bookings;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Booking.Application.Features.Bookings.ConfirmBooking;

public class ConfirmBookingCommandHandler
    : IRequestHandler<ConfirmBookingCommand, Unit>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<ConfirmBookingCommandHandler> _logger;

    public ConfirmBookingCommandHandler(
        IBookingRepository bookingRepository,
        IUserRepository userRepository,
        IEmailService emailService,
        ILogger<ConfirmBookingCommandHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Unit> Handle(
        ConfirmBookingCommand request,
        CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(
            request.BookingId,
            cancellationToken);

        if (booking is null)
            throw new NotFoundException("Booking not found.");

        var ownerId = await _bookingRepository.GetPropertyOwnerIdAsync(
            booking.PropertyId,
            cancellationToken);

        if (ownerId is null)
            throw new NotFoundException("Property owner not found.");

        if (ownerId.Value != request.HostId)
            throw new UnauthorizedException("You are not allowed to confirm this booking.");

        if (booking.BookingStatus != BookingStatus.Pending)
            throw new ConflictException("Only pending bookings can be confirmed.");

        booking.Confirm();

        await _bookingRepository.SaveChangesAsync(cancellationToken);

        var guest = await _userRepository.GetByIdAsync(
            booking.GuestId,
            cancellationToken);

        if (guest is not null && !string.IsNullOrWhiteSpace(guest.Email))
        {
            try
            {
                await _emailService.SendAsync(
                    new EmailMessage
                    {
                        To = guest.Email,
                        Subject = "Booking confirmed",
                        Body =
$@"Hello {guest.FirstName},

Good news. Your booking has been confirmed by the host.

Booking details:
Property Id: {booking.PropertyId}
Check-in: {booking.StartDate}
Check-out: {booking.EndDate}
Guests: {booking.GuestCount}
Total price: {booking.TotalPrice}

We wish you a great stay.

Booking Platform"
                    },
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Booking {BookingId} was confirmed, but email could not be sent to {Email}.",
                    booking.Id,
                    guest.Email);
            }
        }

        return Unit.Value;
    }
}