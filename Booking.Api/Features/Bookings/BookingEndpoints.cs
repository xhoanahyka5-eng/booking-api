using Booking.Application.Features.Bookings.CreateBooking;
using Booking.Application.Features.Bookings.ConfirmBooking;
using Booking.Application.Features.Bookings.RejectBooking;
using Booking.Application.Features.Bookings.CancelBooking;
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
                var userIdStr = http.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var guestId))
                    return Results.Unauthorized();

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
                ISender sender,
                CancellationToken ct
            ) =>
            {
                await sender.Send(new ConfirmBookingCommand(id), ct);

                return Results.Ok();
            })
            .RequireAuthorization();


        app.MapPost("/api/v1/bookings/{id}/reject",
            async (
                int id,
                ISender sender,
                CancellationToken ct
            ) =>
            {
                await sender.Send(new RejectBookingCommand(id), ct);

                return Results.Ok();
            })
            .RequireAuthorization();


        app.MapPost("/api/v1/bookings/{id}/cancel",
            async (
                int id,
                ISender sender,
                CancellationToken ct
            ) =>
            {
                await sender.Send(new CancelBookingCommand(id), ct);

                return Results.Ok();
            })
            .RequireAuthorization();
    }
}