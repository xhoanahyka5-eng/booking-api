namespace Booking.Application.Abstractions.Notifications;

public interface INotificationService
{
    Task AddAsync(
        Guid userId,
        string type,
        string title,
        string message,
        CancellationToken cancellationToken = default);
}