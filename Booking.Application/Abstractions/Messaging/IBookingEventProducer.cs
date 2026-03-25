using Booking.Application.Common.Events;

namespace Booking.Application.Abstractions.Messaging;

public interface IBookingEventProducer
{
    Task PublishAsync(BookingEventMessage message, CancellationToken cancellationToken = default);
}