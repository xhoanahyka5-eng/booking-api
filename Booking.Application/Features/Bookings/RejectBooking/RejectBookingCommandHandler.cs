using Booking.Application.Abstractions.Email;
using Booking.Application.Abstractions.Logging;
using Booking.Application.Abstractions.Messaging;
using Booking.Application.Abstractions.Notifications;
using Booking.Application.Abstractions.Payments;
using Booking.Application.Common.Events;
using Booking.Application.Common.Exceptions;
using Booking.Application.Common.Logging;
using Booking.Application.Common.Payments;
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
    private readonly INotificationService _notificationService;
    private readonly IBookingPaymentService _paymentService;
    private readonly ILogger<RejectBookingCommandHandler> _logger;
    private readonly ILiveNotificationService _liveNotificationService;
    private readonly IKafkaLogProducer _kafkaLogProducer;
    private readonly IBookingEventProducer _bookingEventProducer;

    public RejectBookingCommandHandler(
        IBookingRepository bookingRepository,
        IUserRepository userRepository,
        IEmailService emailService,
        INotificationService notificationService,
        IBookingPaymentService paymentService,
        ILogger<RejectBookingCommandHandler> logger,
        ILiveNotificationService liveNotificationService,
        IKafkaLogProducer kafkaLogProducer,
        IBookingEventProducer bookingEventProducer)
    {
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _notificationService = notificationService;
        _paymentService = paymentService;
        _logger = logger;
        _liveNotificationService = liveNotificationService;
        _kafkaLogProducer = kafkaLogProducer;
        _bookingEventProducer = bookingEventProducer;
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

        await _bookingRepository.RestoreAvailabilityAsync(
            booking.PropertyId,
            booking.StartDate,
            booking.EndDate,
            cancellationToken);

        await _bookingRepository.SaveChangesAsync(cancellationToken);

        var payment = await _paymentService.GetByBookingIdAsync(booking.Id, cancellationToken);
        BookingRefundOutcome? refund = null;

        if (payment is not null && payment.Status == BookingPaymentStatus.Paid)
        {
            refund = CancellationOutcomeCalculator.CalculateHostCancellation(payment.Amount);

            await _paymentService.RegisterRefundAsync(
                booking.Id,
                refund.RefundAmount,
                refund.PenaltyAmount,
                refund.Reason,
                cancellationToken);
        }

        var guest = await _userRepository.GetByIdAsync(booking.GuestId, cancellationToken);

        var propertyName = booking.Property?.Name ?? "your booking";
        var startDateText = booking.StartDate.ToString("dd/MM/yyyy");
        var endDateText = booking.EndDate.ToString("dd/MM/yyyy");

        if (guest is not null && !string.IsNullOrWhiteSpace(guest.Email))
        {
            try
            {
                var refundText = refund is null
                    ? string.Empty
                    : $"\nRefund: {refund.RefundAmount:0.00} {payment!.Currency}";

                var refundHtml = refund is null
                    ? string.Empty
                    : $@"
                                <tr>
                                    <td style=""padding:8px 0; font-size:14px; color:#111827;"">
                                        <strong>Refund:</strong> {refund.RefundAmount:0.00} {payment!.Currency}
                                    </td>
                                </tr>";

                await _emailService.SendAsync(
                    new EmailMessage
                    {
                        To = guest.Email,
                        Subject = "Your booking request was rejected",
                        PlainTextBody =
$@"Hello {guest.FirstName},

Unfortunately, your booking request was rejected by the host.

Property: {propertyName}
Check-in: {startDateText}
Check-out: {endDateText}{refundText}

You can explore other available properties on our platform.

Best regards,
Booking Platform Team",

                        HtmlBody =
$@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>Booking Rejected</title>
</head>
<body style=""margin:0; padding:0; background-color:#f4f6f8; font-family:Arial, Helvetica, sans-serif; color:#111827;"">
    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""background-color:#f4f6f8; padding:30px 0;"">
        <tr>
            <td align=""center"">
                <table role=""presentation"" width=""600"" cellspacing=""0"" cellpadding=""0"" style=""background-color:#ffffff; border-radius:14px; overflow:hidden; box-shadow:0 4px 16px rgba(0,0,0,0.08);"">
                    
                    <tr>
                        <td style=""background-color:#dc2626; padding:24px 32px; color:#ffffff;"">
                            <h1 style=""margin:0; font-size:24px; font-weight:700;"">Your booking request was rejected</h1>
                        </td>
                    </tr>

                    <tr>
                        <td style=""padding:32px;"">
                            <p style=""margin:0 0 16px 0; font-size:16px; line-height:1.6;"">
                                Hello <strong>{guest.FirstName}</strong>,
                            </p>

                            <p style=""margin:0 0 16px 0; font-size:15px; line-height:1.7; color:#374151;"">
                                Unfortunately, your booking request was not approved by the host.
                            </p>

                            <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""margin:24px 0; background-color:#f9fafb; border:1px solid #e5e7eb; border-radius:10px; padding:16px;"">
                                <tr>
                                    <td style=""padding:8px 0; font-size:14px; color:#111827;"">
                                        <strong>Property:</strong> {propertyName}
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
                                {refundHtml}
                            </table>

                            <p style=""margin:0 0 16px 0; font-size:15px; line-height:1.7; color:#374151;"">
                                You can explore other available properties on our platform.
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
                    "Booking {BookingId} was rejected, but email could not be sent to {Email}.",
                    booking.Id,
                    guest.Email);
            }
        }

        var refundMessage = refund is null
            ? string.Empty
            : $" Refund amount: {refund.RefundAmount:0.00} {payment!.Currency}.";

        var notificationMessage =
            $"Your booking for {propertyName} from {startDateText} to {endDateText} was rejected by the host.{refundMessage}";

        try
        {
            await _notificationService.AddAsync(
                booking.GuestId,
                "booking-rejected",
                "Booking rejected",
                notificationMessage,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Booking {BookingId} was rejected, but in-app notification could not be saved for guest {GuestId}.",
                booking.Id,
                booking.GuestId);
        }

        try
        {
            await _liveNotificationService.SendToUserAsync(
                booking.GuestId,
                "booking-rejected",
                "Booking rejected",
                notificationMessage,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Booking {BookingId} was rejected, but live notification could not be sent to guest {GuestId}.",
                booking.Id,
                booking.GuestId);
        }

        await _kafkaLogProducer.PublishAsync(new LogMessage
        {
            Level = "Warning",
            Message = $"Booking {booking.Id} rejected by host.",
            UserId = booking.GuestId.ToString(),
            TraceId = Guid.NewGuid().ToString()
        }, cancellationToken);

        await _bookingEventProducer.PublishAsync(new BookingEventMessage
        {
            EventType = "booking.rejected",
            BookingId = booking.Id,
            PropertyId = booking.PropertyId,
            GuestId = booking.GuestId.ToString(),
            HostId = ownerId.Value.ToString(),
            Status = "Rejected",
            OccurredAtUtc = DateTime.UtcNow
        }, cancellationToken);

        return Unit.Value;
    }
}