using Booking.Application.Common.Logging;

namespace Booking.Application.Abstractions.Logging;

public interface IKafkaLogProducer
{
    Task PublishAsync(LogMessage logMessage, CancellationToken cancellationToken = default);
}