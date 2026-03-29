namespace Booking.Domain.Entities.Payments;

public class BookingPayment
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "EUR";

    public BookingPaymentStatus Status { get; set; }

    public decimal? RefundAmount { get; set; }

    public decimal? PenaltyAmount { get; set; }

    public string? RefundReason { get; set; }

    public DateTime? RefundedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}

public enum BookingPaymentStatus
{
    Pending = 0,
    Paid = 1,
    Refunded = 2
}
