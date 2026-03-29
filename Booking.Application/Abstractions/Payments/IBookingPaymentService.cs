using Booking.Application.Common.Payments;

namespace Booking.Application.Abstractions.Payments;

public interface IBookingPaymentService
{
    Task UpsertPaidAsync(
        int bookingId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default);

    Task<BookingPaymentRecord?> GetByBookingIdAsync(int bookingId, CancellationToken cancellationToken = default);

    Task RegisterRefundAsync(
        int bookingId,
        decimal refundAmount,
        decimal penaltyAmount,
        string reason,
        CancellationToken cancellationToken = default);
}
