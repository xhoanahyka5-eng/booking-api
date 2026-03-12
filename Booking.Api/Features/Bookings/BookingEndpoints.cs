using Booking.Application.Features.Bookings.CancelBooking;
using Booking.Application.Features.Bookings.ConfirmBooking;
using Booking.Application.Features.Bookings.CreateBooking;
using Booking.Application.Features.Bookings.GetHostBookings;
using Booking.Application.Features.Bookings.GetMyBookings;
using Booking.Application.Features.Bookings.RejectBooking;
using MediatR;
using System.Security.Claims;

namespace Booking.Api.Features.Bookings;

public static class BookingEndpoints
{
    public static void MapBookingEndpoints(this WebApplication app)
    {
        app.MapPost("/api/v1/bookings",
            async (
                CreateBookingDto dto,
                HttpContext http,
                ISender sender,
                CancellationToken ct
            ) =>
            {
                var userIdStr =
                    http.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                    http.User.FindFirst("sub")?.Value;

                if (string.IsNullOrWhiteSpace(userIdStr) ||
                    !Guid.TryParse(userIdStr, out var guestId))
                {
                    return Results.Unauthorized();
                }

                var command = new CreateBookingCommand(
                    guestId,
                    dto.PropertyId,
                    dto.StartDate,
                    dto.EndDate,
                    dto.GuestCount
                );

                var bookingId = await sender.Send(command, ct);

                return Results.Ok(new { bookingId });
            })
            .RequireAuthorization();

        app.MapPost("/api/v1/bookings/{id}/confirm",
            async (
                int id,
                HttpContext http,
                ISender sender,
                CancellationToken ct
            ) =>
            {
                var userIdStr =
                    http.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                    http.User.FindFirst("sub")?.Value;

                if (string.IsNullOrWhiteSpace(userIdStr) ||
                    !Guid.TryParse(userIdStr, out var hostId))
                {
                    return Results.Unauthorized();
                }

                await sender.Send(new ConfirmBookingCommand(id, hostId), ct);

                return Results.Ok();
            })
            .RequireAuthorization();

        app.MapPost("/api/v1/bookings/{id}/reject",
            async (
                int id,
                HttpContext http,
                ISender sender,
                CancellationToken ct
            ) =>
            {
                var userIdStr =
                    http.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                    http.User.FindFirst("sub")?.Value;

                if (string.IsNullOrWhiteSpace(userIdStr) ||
                    !Guid.TryParse(userIdStr, out var hostId))
                {
                    return Results.Unauthorized();
                }

                await sender.Send(new RejectBookingCommand(id, hostId), ct);

                return Results.Ok();
            })
            .RequireAuthorization();

        app.MapPost("/api/v1/bookings/{id}/cancel",
            async (
                int id,
                HttpContext http,
                ISender sender,
                CancellationToken ct
            ) =>
            {
                var userIdStr =
                    http.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                    http.User.FindFirst("sub")?.Value;

                if (string.IsNullOrWhiteSpace(userIdStr) ||
                    !Guid.TryParse(userIdStr, out var guestId))
                {
                    return Results.Unauthorized();
                }

                await sender.Send(new CancelBookingCommand(id, guestId), ct);

                return Results.Ok();
            })
            .RequireAuthorization();

        app.MapGet("/api/v1/bookings/my",
            async (
                string? status,
                string? scope,
                HttpContext http,
                ISender sender,
                CancellationToken ct,
                int pageNumber = 1,
                int pageSize = 10
            ) =>
            {
                var userIdStr =
                    http.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                    http.User.FindFirst("sub")?.Value;

                if (string.IsNullOrWhiteSpace(userIdStr) ||
                    !Guid.TryParse(userIdStr, out var guestId))
                {
                    return Results.Unauthorized();
                }

                var query = new GetMyBookingsQuery(
                    guestId,
                    status,
                    scope,
                    pageNumber,
                    pageSize
                );

                var result = await sender.Send(query, ct);

                return Results.Ok(result);
            })
            .RequireAuthorization();

        app.MapGet("/api/v1/bookings/host",
            async (
                string? status,
                string? scope,
                HttpContext http,
                ISender sender,
                CancellationToken ct,
                int pageNumber = 1,
                int pageSize = 10
            ) =>
            {
                var userIdStr =
                    http.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                    http.User.FindFirst("sub")?.Value;

                if (string.IsNullOrWhiteSpace(userIdStr) ||
                    !Guid.TryParse(userIdStr, out var hostId))
                {
                    return Results.Unauthorized();
                }

                var query = new GetHostBookingsQuery(
                    hostId,
                    status,
                    scope,
                    pageNumber,
                    pageSize
                );

                var result = await sender.Send(query, ct);

                return Results.Ok(result);
            })
            .RequireAuthorization();
    }
}