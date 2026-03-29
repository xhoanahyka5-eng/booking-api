using Booking.Application.Abstractions.Email;
using Booking.Application.Abstractions.Logging;
using Booking.Application.Abstractions.Messaging;
using Booking.Application.Abstractions.Notifications;
using Booking.Application.Abstractions.Payments;
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
    private readonly INotificationService _notificationService;
    private readonly ILogger<ConfirmBookingCommandHandler> _logger;
    private readonly ILiveNotificationService _liveNotificationService;
    private readonly IKafkaLogProducer _kafkaLogProducer;
    private readonly IBookingEventProducer _bookingEventProducer;
    private readonly IBookingPaymentService _bookingPaymentService;

    public ConfirmBookingCommandHandler(
        IBookingRepository bookingRepository,
        IUserRepository userRepository,
        IEmailService emailService,
        INotificationService notificationService,
        ILogger<ConfirmBookingCommandHandler> logger,
        ILiveNotificationService liveNotificationService,
        IKafkaLogProducer kafkaLogProducer,
        IBookingEventProducer bookingEventProducer,
        IBookingPaymentService bookingPaymentService)
    {
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _notificationService = notificationService;
        _logger = logger;
        _liveNotificationService = liveNotificationService;
        _kafkaLogProducer = kafkaLogProducer;
        _bookingEventProducer = bookingEventProducer;
        _bookingPaymentService = bookingPaymentService;
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

        var exists = await _bookingRepository.ExistsConfirmedOverlapAsync(
            booking.PropertyId,
            booking.StartDate,
            booking.EndDate,
            booking.Id,
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

        await _bookingPaymentService.UpsertPaidAsync(
            booking.Id,
            booking.TotalPrice,
            "EUR",
            cancellationToken);

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
                    Subject = "Your booking has been confirmed",
                    PlainTextBody =
$@"Hello {guest.FirstName},

Great news — your booking has been confirmed successfully.

Property: {propertyName}
City: {city}
Check-in: {startDateText}
Check-out: {endDateText}
Guests: {booking.GuestCount}
Total price: {booking.TotalPrice:0.00} EUR

We look forward to hosting you.

Best regards,
Booking Platform Team",

                    HtmlBody =
$@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>Booking Confirmed</title>
</head>
<body style=""margin:0; padding:0; background-color:#f4f6f8; font-family:Arial, Helvetica, sans-serif; color:#111827;"">
    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""background-color:#f4f6f8; padding:30px 0;"">
        <tr>
            <td align=""center"">
                <table role=""presentation"" width=""600"" cellspacing=""0"" cellpadding=""0"" style=""background-color:#ffffff; border-radius:14px; overflow:hidden; box-shadow:0 4px 16px rgba(0,0,0,0.08);"">
                    
                    <tr>
                        <td style=""background-color:#16a34a; padding:24px 32px; color:#ffffff;"">
                            <h1 style=""margin:0; font-size:24px; font-weight:700;"">Your booking has been confirmed</h1>
                        </td>
                    </tr>

                    <tr>
                        <td style=""padding:32px;"">
                            <p style=""margin:0 0 16px 0; font-size:16px; line-height:1.6;"">
                                Hello <strong>{guest.FirstName}</strong>,
                            </p>

                            <p style=""margin:0 0 16px 0; font-size:15px; line-height:1.7; color:#374151;"">
                                Great news — your booking has been confirmed successfully.
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
                                <tr>
                                    <td style=""padding:8px 0; font-size:14px; color:#111827;"">
                                        <strong>Guests:</strong> {booking.GuestCount}
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding:8px 0; font-size:14px; color:#111827;"">
                                        <strong>Total price:</strong> {booking.TotalPrice:0.00} EUR
                                    </td>
                                </tr>
                            </table>

                            <p style=""margin:0 0 16px 0; font-size:15px; line-height:1.7; color:#374151;"">
                                We look forward to hosting you.
                            </p>

                            <p style=""margin:0; font-size:15px; line-height:1.7; color:#374151;"">
                                If you have any questions or need assistance before your stay, we are here to help.
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
            HostId = ownerId.Value.ToString(),
            Status = "Confirmed",
            OccurredAtUtc = DateTime.UtcNow
        }, cancellationToken);

        return Unit.Value;
    }
}