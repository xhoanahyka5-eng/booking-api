using Booking.Application.Features.Reviews.CreateReview;
using Booking.Application.Features.Reviews.GetPropertyReviews;
using MediatR;
using System.Security.Claims;

namespace Booking.Api.Features.Reviews;

public static class ReviewEndpoints
{
    public static void MapReviewEndpoints(this WebApplication app)
    {
        app.MapPost("/api/v1/reviews",
            async (
                CreateReviewDto dto,
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

                var command = new CreateReviewCommand(
                    guestId,
                    dto.BookingId,
                    dto.Rating,
                    dto.Comment
                );

                var reviewId = await sender.Send(command, ct);

                return Results.Ok(new { reviewId });
            })
            .RequireAuthorization();

        app.MapGet("/api/v1/reviews/property/{propertyId}",
            async (
                int propertyId,
                ISender sender,
                CancellationToken ct
            ) =>
            {
                var result = await sender.Send(
                    new GetPropertyReviewsQuery(propertyId),
                    ct);

                return Results.Ok(result);
            });
    }
}