using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Booking.Infrastructure.SignalR;

[Authorize]
public class NotificationHub : Hub
{
}