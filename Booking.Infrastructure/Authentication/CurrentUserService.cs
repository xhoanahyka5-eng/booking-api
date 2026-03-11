using Booking.Application.Abstractions.Authentication;
using Booking.Application.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Booking.Infrastructure.Authentication;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;

            var userId =
                user?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                user?.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
                throw new UnauthorizedException("User is not authenticated.");

            return parsedUserId;
        }
    }
}