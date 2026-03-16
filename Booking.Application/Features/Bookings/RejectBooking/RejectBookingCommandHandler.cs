using Booking.Application.Abstractions.Email;
using Booking.Application.Common.Exceptions;
using Booking.Application.Features.Bookings.Persistence;
using Booking.Application.Features.Users.Persistence;
using Booking.Domain.Entities.Bookings;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Booking.Application.Features.Bookings.RejectBooking;

public class RejectBookingCommandHandler
    : IRequestHandler<RejectBookingCommand, Unit>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<RejectBookingCommandHandler> _logger;

    public RejectBookingCommandHandler(
        IBookingRepository bookingRepository,
        IUserRepository userRepository,
        IEmailService emailService,
        ILogger<RejectBookingCommandHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Unit> Handle(
        RejectBookingCommand request,
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
            throw new UnauthorizedException("You are not allowed to reject this booking.");

        if (booking.BookingStatus != BookingStatus.Pending)
            throw new ConflictException("Only pending bookings can be rejected.");

        booking.Reject();

        await _bookingRepository.SaveChangesAsync(cancellationToken);

        var guest = await _userRepository.GetByIdAsync(
            booking.GuestId,
            cancellationToken);

        if (guest is not null && !string.IsNullOrWhiteSpace(guest.Email))
        {
            try
            {
                _logger.LogInformation(
                    "Sending rejection email to {Email} for booking {BookingId}",
                    guest.Email,
                    booking.Id);

                await _emailService.SendAsync(
                    new EmailMessage
                    {
                        To = guest.Email,
                        Subject = "Update on your booking request",
                        Body =
$@"Hello {guest.FirstName},

We are sorry, but your booking request was not accepted by the host.

Booking details:
Property ID: {booking.PropertyId}
Check-in: {booking.StartDate:dd/MM/yyyy}
Check-out: {booking.EndDate:dd/MM/yyyy}
Guests: {booking.GuestCount}
Total price: {booking.TotalPrice:0.00}

You can search for other available properties and place a new booking request.

Thank you for using our Booking Platform."
                    },
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Booking {BookingId} was rejected, but email could not be sent to {Email}.",
                    booking.Id,
                    guest.Email);
            }
        }

        return Unit.Value;
    }
}