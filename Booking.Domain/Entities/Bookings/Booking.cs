using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Booking.Domain.Entities.Bookings;

public class Booking
{
    public int Id { get; private set; }

    public int PropertyId { get; private set; }
    public Guid GuestId { get; private set; }

    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }

    public int GuestCount { get; private set; }

    public decimal CleaningFee { get; private set; }
    public decimal AmenitiesUpCharge { get; private set; }
    public decimal PriceForPeriod { get; private set; }

    [NotMapped]
    public decimal TotalPrice =>
        PriceForPeriod + CleaningFee + AmenitiesUpCharge;

    public BookingStatus BookingStatus { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? LastModifiedAt { get; private set; }

    public DateTime? ConfirmedOnUtc { get; private set; }
    public DateTime? RejectedOnUtc { get; private set; }
    public DateTime? CompletedOnUtc { get; private set; }
    public DateTime? CancelledOnUtc { get; private set; }

    private Booking() { }

    public Booking(int propertyId, Guid guestId, DateOnly start, DateOnly end, int guestCount)
    {
        if (end <= start)
            throw new ArgumentException("End date must be after start date.");

        if (guestCount <= 0)
            throw new ArgumentException("Guest count must be greater than zero.");

        PropertyId = propertyId;
        GuestId = guestId;
        StartDate = start;
        EndDate = end;
        GuestCount = guestCount;

        CreatedAt = DateTime.UtcNow;
        BookingStatus = BookingStatus.Pending;
    }

    public void SetPricing(decimal priceForPeriod, decimal cleaningFee = 0, decimal amenitiesUpCharge = 0)
    {
        if (priceForPeriod < 0 || cleaningFee < 0 || amenitiesUpCharge < 0)
            throw new ArgumentException("Pricing values cannot be negative.");

        PriceForPeriod = priceForPeriod;
        CleaningFee = cleaningFee;
        AmenitiesUpCharge = amenitiesUpCharge;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Confirm()
    {
        if (BookingStatus != BookingStatus.Pending)
            throw new Exception("Booking cannot be confirmed.");

        BookingStatus = BookingStatus.Confirmed;
        ConfirmedOnUtc = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Reject()
    {
        if (BookingStatus != BookingStatus.Pending)
            throw new InvalidOperationException("Booking cannot be rejected.");

        BookingStatus = BookingStatus.Rejected;
        RejectedOnUtc = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (BookingStatus != BookingStatus.Pending &&
            BookingStatus != BookingStatus.Confirmed)
            throw new InvalidOperationException("Booking cannot be cancelled.");

        BookingStatus = BookingStatus.Cancelled;
        CancelledOnUtc = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }
}

public enum BookingStatus
{
    Pending = 0,
    Confirmed = 1,
    Rejected = 2,
    Completed = 3,
    Cancelled = 4
}