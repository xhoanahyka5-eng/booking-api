using Booking.Application.Abstractions.Email;
using Booking.Application.Abstractions.Logging;
using Booking.Application.Abstractions.Messaging;
using Booking.Application.Abstractions.Notifications;
using Booking.Application.Common.Emails;
using Booking.Application.Common.Events;
using Booking.Application.Common.Exceptions;
using Booking.Application.Common.Logging;
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
    private readonly ILiveNotificationService _liveNotificationService;
    private readonly IKafkaLogProducer _kafkaLogProducer;
    private readonly IBookingEventProducer _bookingEventProducer;

    public ConfirmBookingCommandHandler(
        IBookingRepository bookingRepository,
        IUserRepository userRepository,
        IEmailService emailService,
        INotificationService notificationService,
        ILogger<ConfirmBookingCommandHandler> logger,
        ILiveNotificationService liveNotificationService,
        IKafkaLogProducer kafkaLogProducer)
    {
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _logger = logger;
        _liveNotificationService = liveNotificationService;
        _kafkaLogProducer = kafkaLogProducer;
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

        var exists = await _bookingRepository.ExistsAsync(
            booking.PropertyId,
            booking.StartDate,
            booking.EndDate,
            cancellationToken);

        if (exists)
            throw new ConflictException("These dates are already booked.");

        booking.Confirm();

        await _bookingRepository.BlockAvailabilityAsync(
            booking.PropertyId,
            booking.StartDate,
            booking.EndDate,
            cancellationToken);

        await _bookingRepository.SaveChangesAsync(cancellationToken);

        var guest = await _userRepository.GetByIdAsync(booking.GuestId, cancellationToken);

        var propertyName = booking.Property?.Name ?? "your property";
        var city = booking.Property?.Address?.City ?? "your destination";
        var startDateText = booking.StartDate.ToString("dd/MM/yyyy");
        var endDateText = booking.EndDate.ToString("dd/MM/yyyy");

        if (guest is not null && !string.IsNullOrWhiteSpace(guest.Email))
        {
            try
            {
                await _emailService.SendAsync(new EmailMessage
                {
                    To = guest.Email,
                    Subject = "Booking confirmed",
                    Body = BookingEmailTemplates.BuildBookingConfirmedBody(
                        guest.FirstName,
                        propertyName,
                        city,
                        booking.StartDate,
                        booking.EndDate,
                        booking.GuestCount,
                        booking.TotalPrice,
                        booking.Property.CheckInTime)
                }, cancellationToken);
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

        try
        {
            await _notificationService.AddAsync(
                booking.GuestId,
                "booking-confirmed",
                "Booking confirmed",
                $"Your booking for {propertyName} from {startDateText} to {endDateText} was confirmed.",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Booking {BookingId} was confirmed, but in-app notification could not be saved for guest {GuestId}.",
                booking.Id,
                booking.GuestId);
        }

        try
        {
            await _liveNotificationService.SendToUserAsync(
                booking.GuestId,
                "booking-confirmed",
                "Booking confirmed",
                $"Your booking for {propertyName} from {startDateText} to {endDateText} was confirmed.",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Booking {BookingId} was confirmed, but live notification could not be sent to guest {GuestId}.",
                booking.Id,
                booking.GuestId);
        }

        await _kafkaLogProducer.PublishAsync(new LogMessage
        {
            Level = "Information",
            Message = $"Booking {booking.Id} confirmed successfully.",
            UserId = booking.GuestId.ToString(),
            TraceId = Guid.NewGuid().ToString()
        }, cancellationToken);

        await _bookingEventProducer.PublishAsync(new BookingEventMessage
        {
            EventType = "booking.confirmed",
            BookingId = booking.Id,
            PropertyId = booking.PropertyId,
            GuestId = booking.GuestId.ToString(),
            HostId = booking.Property.OwnerId.ToString(),
            Status = "Confirmed",
            OccurredAtUtc = DateTime.UtcNow
        }, cancellationToken);

        return Unit.Value;
    }
}