using System;
using System.Threading;
using System.Threading.Tasks;
using Booking.Application.Abstractions.Payments;
using Booking.Application.Common.Payments;
using Booking.Domain.Entities.Payments;
using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DomainPaymentStatus = Booking.Domain.Entities.Payments.BookingPaymentStatus;

namespace Booking.Infrastructure.Payments;

public sealed class BookingPaymentService : IBookingPaymentService
{
    private readonly BookingDbContext _db;
    private readonly ILogger<BookingPaymentService> _logger;

    public BookingPaymentService(BookingDbContext db, ILogger<BookingPaymentService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task UpsertPaidAsync(
        int bookingId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default)
    {
        var existing = await _db.BookingPayments
            .FirstOrDefaultAsync(p => p.BookingId == bookingId, cancellationToken);

        if (existing is null)
        {
            _db.BookingPayments.Add(new BookingPayment
            {
                BookingId = bookingId,
                Amount = amount,
                Currency = string.IsNullOrWhiteSpace(currency) ? "EUR" : currency,
                Status = DomainPaymentStatus.Paid,
                CreatedAtUtc = DateTime.UtcNow
            });
        }
        else
        {
            existing.Amount = amount;
            existing.Currency = string.IsNullOrWhiteSpace(currency) ? "EUR" : currency;
            existing.Status = DomainPaymentStatus.Paid;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<BookingPaymentRecord?> GetByBookingIdAsync(int bookingId, CancellationToken cancellationToken = default)
    {
        var row = await _db.BookingPayments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.BookingId == bookingId, cancellationToken);

        if (row is null)
            return null;

        return new BookingPaymentRecord
        {
            BookingId = row.BookingId,
            Amount = row.Amount,
            Currency = row.Currency,
            Status = (Booking.Application.Common.Payments.BookingPaymentStatus)(int)row.Status
        };
    }

    public async Task RegisterRefundAsync(
        int bookingId,
        decimal refundAmount,
        decimal penaltyAmount,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var row = await _db.BookingPayments
            .FirstOrDefaultAsync(p => p.BookingId == bookingId, cancellationToken);

        if (row is null)
        {
            _logger.LogWarning("No payment row for booking {BookingId}; refund skipped.", bookingId);
            return;
        }

        row.RefundAmount = refundAmount;
        row.PenaltyAmount = penaltyAmount;
        row.RefundReason = reason;
        row.RefundedAtUtc = DateTime.UtcNow;
        row.Status = DomainPaymentStatus.Refunded;

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Refund persisted for booking {BookingId}. Refund={Refund}, Penalty={Penalty}",
            bookingId,
            refundAmount,
            penaltyAmount);
    }
}
