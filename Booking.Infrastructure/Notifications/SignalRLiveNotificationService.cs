using Booking.Application.Abstractions.Notifications;
using Booking.Infrastructure.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace Booking.Infrastructure.Notifications;

public class SignalRLiveNotificationService : ILiveNotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRLiveNotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendToUserAsync(
        Guid userId,
        string type,
        string title,
        string message,
        CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.User(userId.ToString()).SendAsync(
            "ReceiveNotification",
            new
            {
                type,
                title,
                message,
                createdAt = DateTime.UtcNow
            },
            cancellationToken);
    }
}