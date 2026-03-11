using Booking.Application.Common.Exceptions;
using Booking.Application.Features.Bookings.Persistence;
using MediatR;
using BookingEntity = Booking.Domain.Entities.Bookings.Booking;

namespace Booking.Application.Features.Bookings.CreateBooking;

public class CreateBookingCommandHandler
    : IRequestHandler<CreateBookingCommand, int>
{
    private readonly IBookingRepository _bookingRepository;

    public CreateBookingCommandHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<int> Handle(
        CreateBookingCommand request,
        CancellationToken cancellationToken)
    {
        var property = await _bookingRepository.GetPropertyWithAvailabilityAsync(
            request.PropertyId,
            cancellationToken);

        if (property is null)
            throw new NotFoundException("Property not found.");

        if (request.GuestCount > property.MaxGuests)
            throw new ConflictException("Guest count exceeds property capacity.");

        var requestedDates = Enumerable
            .Range(0, request.EndDate.DayNumber - request.StartDate.DayNumber)
            .Select(offset => request.StartDate.AddDays(offset))
            .ToList();

        var availableDates = property.Availabilities
            .Where(a => requestedDates.Contains(a.Date) && a.IsAvailable)
            .ToList();

        if (availableDates.Count != requestedDates.Count)
            throw new ConflictException("Selected dates are not fully available.");

        var totalPrice = availableDates.Sum(x => x.Price);

        var booking = new BookingEntity(
            request.PropertyId,
            request.GuestId,
            request.StartDate,
            request.EndDate,
            request.GuestCount
        );

        booking.SetPricing(totalPrice);

        return await _bookingRepository.AddBookingAsync(booking, cancellationToken);
    }
}