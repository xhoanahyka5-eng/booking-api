using Booking.Application.Abstractions.Email;
using Booking.Application.Abstractions.Logging;
using Booking.Application.Abstractions.Messaging;
using Booking.Application.Abstractions.Notifications;
using Booking.Application.Common.Bookings;
using Booking.Application.Common.Emails;
using Booking.Application.Common.Events;
using Booking.Application.Common.Exceptions;
using Booking.Application.Common.Logging;
using Booking.Application.Common.Properties;
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
    private readonly INotificationService _notificationService;
    private readonly ILiveNotificationService _liveNotificationService;
    private readonly ILogger<CreateBookingCommandHandler> _logger;
    private readonly IKafkaLogProducer _kafkaLogProducer;
    private readonly IBookingEventProducer _bookingEventProducer;

    public CreateBookingCommandHandler(
        IBookingRepository bookingRepository,
        IUserRepository userRepository,
        IEmailService emailService,
        INotificationService notificationService,
        ILiveNotificationService liveNotificationService,
        ILogger<CreateBookingCommandHandler> logger,
        IKafkaLogProducer kafkaLogProducer)

    {
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _notificationService = notificationService;
        _liveNotificationService = liveNotificationService;
        _logger = logger;
        _kafkaLogProducer = kafkaLogProducer;
    }

    public async Task<int> Handle(
        CreateBookingCommand request,
        CancellationToken cancellationToken)
    {
        // ✅ 1. Date validation
        if (request.StartDate >= request.EndDate)
            throw new ConflictException("Invalid date range.");

        if (request.StartDate < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ConflictException("Cannot book in the past.");

        var property = await _bookingRepository.GetPropertyWithAvailabilityAsync(
            request.PropertyId,
            cancellationToken);

        if (property is null)
            throw new NotFoundException("Property not found.");

        // ✅ 2. Guest count validation
        if (request.GuestCount > property.MaxGuests)
            throw new ConflictException("Guest count exceeds property capacity.");

        var requestedDates = Enumerable
            .Range(0, request.EndDate.DayNumber - request.StartDate.DayNumber)
            .Select(offset => request.StartDate.AddDays(offset))
            .ToList();

        var nights = requestedDates.Count;
        var policy = PropertyPolicyCodec.Parse(property.Rules);

        var availableDates = property.Availabilities
            .Where(a => requestedDates.Contains(a.Date) && a.IsAvailable)
            .ToList();

        if (policy.MaximumStayNights.HasValue && nights > policy.MaximumStayNights.Value)
            throw new ConflictException($"Maximum stay is {policy.MaximumStayNights.Value} night(s).");

        var availableDates = property.Availabilities
            .Where(a => requestedDates.Contains(a.Date) && a.IsAvailable)
            .ToList();

        if (availableDates.Count != requestedDates.Count)
            throw new ConflictException("Selected dates are not fully available.");

        var alreadyBooked = await _bookingRepository.ExistsAsync(
            request.PropertyId,
            request.StartDate,
            request.EndDate,
            cancellationToken);

        if (alreadyBooked)
            throw new ConflictException("Property already booked for selected dates.");

        var totalPrice = availableDates.Sum(x => x.Price);

        var booking = new BookingEntity(
            request.PropertyId,
            request.GuestId,
            request.StartDate,
            request.EndDate,
            request.GuestCount
        );

        var booking = new BookingEntity(
            request.PropertyId,
            request.GuestId,
            request.StartDate,
            request.EndDate,
            request.GuestCount);

        booking.SetPricing(
            pricing.PriceForPeriod,
            pricing.CleaningFee,
            pricing.AdditionalGuestFees,
            pricing.ServiceFee,
            pricing.Tax,
            pricing.Discount);

        var bookingId = await _bookingRepository.AddBookingAsync(booking, cancellationToken);

        // ✅ 4. Update availability
        foreach (var date in requestedDates)
        {
            await _bookingRepository.MarkUnavailableAsync(
                request.PropertyId,
                date,
                cancellationToken);
        }

        await _bookingRepository.SaveChangesAsync(cancellationToken);

        // ✅ 5. Email
        var guest = await _userRepository.GetByIdAsync(
            request.GuestId,
            cancellationToken);

        var propertyName = property.Name;
        var startDateText = request.StartDate.ToString("dd/MM/yyyy");
        var endDateText = request.EndDate.ToString("dd/MM/yyyy");

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

Property: {propertyName}
Check-in: {startDateText}
Check-out: {endDateText}
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
                    "Booking {BookingId} was created, but email could not be sent to guest {GuestId}.",
                    bookingId,
                    request.GuestId);
            }
        }

        try
        {
            await _notificationService.AddAsync(
                request.GuestId,
                "booking-created",
                "Booking request created",
                $"Your request for {propertyName} from {startDateText} to {endDateText} is pending host approval.",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Booking {BookingId} was created, but in-app notification could not be saved for guest {GuestId}.",
                bookingId,
                request.GuestId);
        }

        if (ownerId.HasValue)
        {
            var hostMessage =
                $"A guest requested {propertyName} from {startDateText} to {endDateText}.";

            try
            {
                await _notificationService.AddAsync(
                    ownerId.Value,
                    "booking-request",
                    "New booking request",
                    hostMessage,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Booking {BookingId} was created, but in-app notification could not be saved for host {HostId}.",
                    bookingId,
                    ownerId.Value);
            }

            try
            {
                await _liveNotificationService.SendToUserAsync(
                    ownerId.Value,
                    "booking-request",
                    "New booking request",
                    hostMessage,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Booking {BookingId} was created, but live notification could not be sent to host {HostId}.",
                    bookingId,
                    ownerId.Value);
            }
        }
        await _kafkaLogProducer.PublishAsync(new LogMessage
        {
            Level = "Information",
            Message = $"Booking {bookingId} created successfully.",
            UserId = request.GuestId.ToString(),
            TraceId = Guid.NewGuid().ToString()
        }, cancellationToken);

        await _bookingEventProducer.PublishAsync(new BookingEventMessage
        {
            EventType = "booking.created",
            BookingId = bookingId,
            PropertyId = request.PropertyId,
            GuestId = request.GuestId.ToString(),
            HostId = ownerId?.ToString(),
            Status = "Pending",
            OccurredAtUtc = DateTime.UtcNow
        }, cancellationToken);

        return bookingId;
    }
}