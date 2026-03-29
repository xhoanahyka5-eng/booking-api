using Booking.Application.Abstractions.Email;
using Booking.Application.Abstractions.Logging;
using Booking.Application.Abstractions.Messaging;
using Booking.Application.Abstractions.Notifications;
using Booking.Application.Common.Events;
using Booking.Application.Common.Exceptions;
using Booking.Application.Common.Logging;
using Booking.Application.Features.Bookings.Persistence;
using Booking.Application.Features.Users.Persistence;
using Booking.Domain.Entities.Bookings;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Booking.Application.Features.Bookings.CancelBooking;

public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;
    private readonly ILiveNotificationService _liveNotificationService;
    private readonly IKafkaLogProducer _kafkaLogProducer;
    private readonly IBookingEventProducer _bookingEventProducer;
    private readonly ILogger<CancelBookingCommandHandler> _logger;

    public CancelBookingCommandHandler(
        IBookingRepository bookingRepository,
        IUserRepository userRepository,
        IEmailService emailService,
        INotificationService notificationService,
        ILiveNotificationService liveNotificationService,
        IKafkaLogProducer kafkaLogProducer,
        IBookingEventProducer bookingEventProducer,
        ILogger<CancelBookingCommandHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _notificationService = notificationService;
        _liveNotificationService = liveNotificationService;
        _kafkaLogProducer = kafkaLogProducer;
        _bookingEventProducer = bookingEventProducer;
        _logger = logger;
    }

    public async Task Handle(
        CancelBookingCommand request,
        CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(
            request.BookingId,
            cancellationToken);

        if (booking is null)
            throw new NotFoundException("Booking not found.");

        if (booking.GuestId != request.GuestId)
            throw new UnauthorizedException("You are not allowed to cancel this booking.");

        var wasConfirmed = booking.BookingStatus == BookingStatus.Confirmed;

        try
        {
            booking.Cancel();
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(ex.Message);
        }

        if (wasConfirmed)
        {
            await _bookingRepository.RestoreAvailabilityAsync(
                booking.PropertyId,
                booking.StartDate,
                booking.EndDate,
                cancellationToken);
        }

        await _bookingRepository.SaveChangesAsync(cancellationToken);

        var guest = await _userRepository.GetByIdAsync(booking.GuestId, cancellationToken);
        var ownerId = await _bookingRepository.GetPropertyOwnerIdAsync(
            booking.PropertyId,
            cancellationToken);

        var propertyName = booking.Property?.Name ?? "your booking";
        var city = booking.Property?.Address?.City ?? "your destination";
        var startDateText = booking.StartDate.ToString("dd/MM/yyyy");
        var endDateText = booking.EndDate.ToString("dd/MM/yyyy");

        if (guest is not null && !string.IsNullOrWhiteSpace(guest.Email))
        {
            try
            {
                await _emailService.SendAsync(
                    new EmailMessage
                    {
                        To = guest.Email,
                        Subject = "Your booking was cancelled",
                        PlainTextBody =
$@"Hello {guest.FirstName},

Your booking has been cancelled successfully.

Property: {propertyName}
City: {city}
Check-in: {startDateText}
Check-out: {endDateText}

If you need help finding another place to stay, you can explore other available properties on our platform.

Best regards,
Booking Platform Team",

                        HtmlBody =
$@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>Booking Cancelled</title>
</head>
<body style=""margin:0; padding:0; background-color:#f4f6f8; font-family:Arial, Helvetica, sans-serif; color:#111827;"">
    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""background-color:#f4f6f8; padding:30px 0;"">
        <tr>
            <td align=""center"">
                <table role=""presentation"" width=""600"" cellspacing=""0"" cellpadding=""0"" style=""background-color:#ffffff; border-radius:14px; overflow:hidden; box-shadow:0 4px 16px rgba(0,0,0,0.08);"">
                    
                    <tr>
                        <td style=""background-color:#f59e0b; padding:24px 32px; color:#ffffff;"">
                            <h1 style=""margin:0; font-size:24px; font-weight:700;"">Your booking was cancelled</h1>
                        </td>
                    </tr>

                    <tr>
                        <td style=""padding:32px;"">
                            <p style=""margin:0 0 16px 0; font-size:16px; line-height:1.6;"">
                                Hello <strong>{guest.FirstName}</strong>,
                            </p>

                            <p style=""margin:0 0 16px 0; font-size:15px; line-height:1.7; color:#374151;"">
                                Your booking has been cancelled successfully.
                            </p>

                            <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""margin:24px 0; background-color:#f9fafb; border:1px solid #e5e7eb; border-radius:10px; padding:16px;"">
                                <tr>
                                    <td style=""padding:8px 0; font-size:14px; color:#111827;"">
                                        <strong>Property:</strong> {propertyName}
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding:8px 0; font-size:14px; color:#111827;"">
                                        <strong>City:</strong> {city}
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding:8px 0; font-size:14px; color:#111827;"">
                                        <strong>Check-in:</strong> {startDateText}
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding:8px 0; font-size:14px; color:#111827;"">
                                        <strong>Check-out:</strong> {endDateText}
                                    </td>
                                </tr>
                            </table>

                            <p style=""margin:0 0 16px 0; font-size:15px; line-height:1.7; color:#374151;"">
                                If you need help finding another place to stay, you can explore other available properties on our platform.
                            </p>

                            <p style=""margin:0; font-size:15px; line-height:1.7; color:#374151;"">
                                If you have any questions or need assistance, we are here to help.
                            </p>
                        </td>
                    </tr>

                    <tr>
                        <td style=""padding:20px 32px; background-color:#f9fafb; border-top:1px solid #e5e7eb; font-size:13px; color:#6b7280;"">
                            Best regards,<br />
                            <strong>Booking Platform Team</strong>
                        </td>
                    </tr>

                </table>
            </td>
        </tr>
    </table>
</body>
</html>"
                    },
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Booking {BookingId} was cancelled, but email could not be sent to {Email}.",
                    booking.Id,
                    guest.Email);
            }
        }

        var guestNotificationMessage =
            $"Your booking for {propertyName} from {startDateText} to {endDateText} was cancelled successfully.";

        try
        {
            await _notificationService.AddAsync(
                booking.GuestId,
                "booking-cancelled",
                "Booking cancelled",
                guestNotificationMessage,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Booking {BookingId} was cancelled, but in-app notification could not be saved for guest {GuestId}.",
                booking.Id,
                booking.GuestId);
        }

        try
        {
            await _liveNotificationService.SendToUserAsync(
                booking.GuestId,
                "booking-cancelled",
                "Booking cancelled",
                guestNotificationMessage,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Booking {BookingId} was cancelled, but live notification could not be sent to guest {GuestId}.",
                booking.Id,
                booking.GuestId);
        }

        if (ownerId is not null)
        {
            var hostNotificationMessage =
                $"A booking for {propertyName} from {startDateText} to {endDateText} was cancelled by the guest.";

            try
            {
                await _notificationService.AddAsync(
                    ownerId.Value,
                    "booking-cancelled",
                    "Booking cancelled",
                    hostNotificationMessage,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Booking {BookingId} was cancelled, but in-app notification could not be saved for host {HostId}.",
                    booking.Id,
                    ownerId.Value);
            }

            try
            {
                await _liveNotificationService.SendToUserAsync(
                    ownerId.Value,
                    "booking-cancelled",
                    "Booking cancelled",
                    hostNotificationMessage,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Booking {BookingId} was cancelled, but live notification could not be sent to host {HostId}.",
                    booking.Id,
                    ownerId.Value);
            }
        }

        await _kafkaLogProducer.PublishAsync(new LogMessage
        {
            Level = "Information",
            Message = $"Booking {booking.Id} cancelled by guest.",
            UserId = booking.GuestId.ToString(),
            TraceId = Guid.NewGuid().ToString()
        }, cancellationToken);

        await _bookingEventProducer.PublishAsync(new BookingEventMessage
        {
            EventType = "booking.cancelled",
            BookingId = booking.Id,
            PropertyId = booking.PropertyId,
            GuestId = booking.GuestId.ToString(),
            HostId = ownerId?.ToString() ?? string.Empty,
            Status = "Cancelled",
            OccurredAtUtc = DateTime.UtcNow
        }, cancellationToken);
    }
}