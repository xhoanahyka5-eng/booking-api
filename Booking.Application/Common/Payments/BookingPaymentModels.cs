namespace Booking.Application.Common.Payments;

public enum BookingPaymentStatus
{
    Pending = 0,
    Paid = 1,
    Refunded = 2
}

// Kept in sync with Booking.Domain.Entities.Payments.BookingPaymentStatus

public sealed class BookingPaymentRecord
{
    public int BookingId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "EUR";

    public BookingPaymentStatus Status { get; set; } = BookingPaymentStatus.Pending;
}

public sealed class BookingRefundOutcome
{
    public decimal RefundAmount { get; set; }

    public decimal PenaltyAmount { get; set; }

    public string Reason { get; set; } = string.Empty;
}

public static class CancellationOutcomeCalculator
{
    // Simple host-cancel policy: full refund to guest, no penalty.
    public static BookingRefundOutcome CalculateHostCancellation(decimal amount)
    {
        var safeAmount = amount < 0 ? 0 : amount;

        return new BookingRefundOutcome
        {
            RefundAmount = safeAmount,
            PenaltyAmount = 0,
            Reason = "Host cancelled booking"
        };
    }
}
