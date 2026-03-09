using Booking.Application.Common.Exceptions;
using Booking.Application.Features.Bookings.Persistence;
using MediatR;
using BookingEntity = Booking.Domain.Entities.Bookings.Booking;

namespace Booking.Application.Features.Bookings.CreateBooking;

public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, int>
{
    private readonly IBookingRepository _bookingRepository;

    public CreateBookingCommandHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<int> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var property = await _bookingRepository.GetPropertyWithAvailabilityAsync(
            request.PropertyId,
            cancellationToken);

        if (property is null)
            throw new NotFoundException("Property not found.");

        if (request.GuestCount > property.MaxGuests)
            throw new ConflictException("Guest count exceeds property capacity.");

        var requestedDates = GetDatesInRange(request.StartDate, request.EndDate);

        var availableEntries = property.Availabilities
            .Where(a => requestedDates.Contains(a.Date) && a.IsAvailable)
            .ToList();

        if (availableEntries.Count != requestedDates.Count)
            throw new ConflictException("Property is not available for all selected dates.");

        var priceForPeriod = availableEntries.Sum(a => a.Price);

        var booking = new BookingEntity(
            request.PropertyId,
            request.GuestId,
            request.StartDate,
            request.EndDate,
            request.GuestCount
        );

        booking.SetPricing(priceForPeriod, 0, 0);
        booking.Confirm();

        foreach (var availability in availableEntries)
        {
            availability.IsAvailable = false;
        }

        var bookingId = await _bookingRepository.AddBookingAsync(booking, cancellationToken);
        await _bookingRepository.SaveChangesAsync(cancellationToken);

        return bookingId;
    }

    private static List<DateOnly> GetDatesInRange(DateOnly startDate, DateOnly endDate)
    {
        var dates = new List<DateOnly>();
        var current = startDate;

        while (current < endDate)
        {
            dates.Add(current);
            current = current.AddDays(1);
        }

        return dates;
    }
}