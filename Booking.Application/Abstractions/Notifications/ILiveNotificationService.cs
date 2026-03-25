namespace Booking.Application.Abstractions.Notifications;

public interface ILiveNotificationService
{
    Task SendToUserAsync(Guid userId, string type, string title, string message, CancellationToken cancellationToken = default);
}