using Booking.Application.Abstractions.Email;
using Booking.Application.Abstractions.Logging;
using Booking.Application.Abstractions.Logging;
using Booking.Application.Abstractions.Messaging;
using Booking.Application.Abstractions.Notifications;
using Booking.Application.Abstractions.Payments;
using Booking.Application.Common.Bookings;
using Booking.Application.Common.Events;
using Booking.Application.Common.Exceptions;
using Booking.Application.Common.Logging;
using Booking.Application.Common.Logging;
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

        // ✅ DOMAIN LOGIC
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
            refund = CancellationOutcomeCalculator.CalculateHostCancellation(booking, payment.Amount);

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
                await _emailService.SendAsync(
                    new EmailMessage
                    {
                        To = guest.Email,
                        Subject = "Booking rejected",
                        Body =
$@"Hello {guest.FirstName},

Unfortunately, your booking request has been rejected by the host.

Property: {propertyName}
Check-in: {startDateText}
Check-out: {endDateText}

You can explore other available properties on our platform.

Booking Platform"
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
            : $" Refund amount: {refund.RefundAmount:0.00} {payment!.Currency}. Penalty: {refund.PenaltyAmount:0.00} {payment.Currency}.";

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
            HostId = booking.Property.OwnerId.ToString(),
            Status = "Rejected",
            OccurredAtUtc = DateTime.UtcNow
        }, cancellationToken);

        return Unit.Value;
    }
}