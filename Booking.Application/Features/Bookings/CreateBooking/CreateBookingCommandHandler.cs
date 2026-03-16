using Booking.Application.Abstractions.Email;
using Booking.Application.Common.Exceptions;
using Booking.Application.Features.Bookings.Persistence;
using Booking.Application.Features.Users.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;
using BookingEntity = Booking.Domain.Entities.Bookings.Booking;

namespace Booking.Application.Features.Bookings.CreateBooking;

public class CreateBookingCommandHandler
    : IRequestHandler<CreateBookingCommand, int>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<CreateBookingCommandHandler> _logger;

    public CreateBookingCommandHandler(
        IBookingRepository bookingRepository,
        IUserRepository userRepository,
        IEmailService emailService,
        ILogger<CreateBookingCommandHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<int> Handle(
        CreateBookingCommand request,
        CancellationToken cancellationToken)
    {
        var property = await _bookingRepository.GetPropertyWithAvailabilityAsync(
            request.PropertyId,
            cancellationToken);

        if (property is null)
            throw new NotFoundException("Property not found.");

        if (request.GuestCount > property.MaxGuests)
            throw new ConflictException("Guest count exceeds property capacity.");

        var requestedDates = Enumerable
            .Range(0, request.EndDate.DayNumber - request.StartDate.DayNumber)
            .Select(offset => request.StartDate.AddDays(offset))
            .ToList();

        var availableDates = property.Availabilities
            .Where(a => requestedDates.Contains(a.Date) && a.IsAvailable)
            .ToList();

        if (availableDates.Count != requestedDates.Count)
            throw new ConflictException("Selected dates are not fully available.");

        var totalPrice = availableDates.Sum(x => x.Price);

        var booking = new BookingEntity(
            request.PropertyId,
            request.GuestId,
            request.StartDate,
            request.EndDate,
            request.GuestCount
        );

        booking.SetPricing(totalPrice);

        var bookingId = await _bookingRepository.AddBookingAsync(
            booking,
            cancellationToken);

        var guest = await _userRepository.GetByIdAsync(
            request.GuestId,
            cancellationToken);

        if (guest is not null && !string.IsNullOrWhiteSpace(guest.Email))
        {
            try
            {
                await _emailService.SendAsync(
                    new EmailMessage
                    {
                        To = guest.Email,
                        Subject = "Booking request created",
                        Body =
$@"Hello {guest.FirstName},

Your booking request was created successfully.

Property: {property.Name}
Check-in: {request.StartDate}
Check-out: {request.EndDate}
Guests: {request.GuestCount}
Total price: {totalPrice}

Your request is currently pending host confirmation.

Booking Platform"
                    },
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Booking {BookingId} was created, but email could not be sent to {Email}.",
                    bookingId,
                    guest.Email);
            }
        }

        return bookingId;
    }
}
