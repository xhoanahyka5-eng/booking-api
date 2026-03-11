using Booking.Application.Features.Properties.AddPropertyPhoto;
using Booking.Application.Features.Properties.ApproveProperty;
using Booking.Application.Features.Properties.CreateProperty;
using Booking.Application.Features.Properties.GetAllProperties;
using Booking.Application.Features.Properties.GetAvailability;
using Booking.Application.Features.Properties.GetPropertyById;
using Booking.Application.Features.Properties.GetPropertyPhotos;
using Booking.Application.Features.Properties.SearchProperties;
using Booking.Application.Features.Properties.SetAvailability;
using Booking.Application.Features.Properties.SetPropertyStatus;
using Booking.Application.Features.Properties.UpdateProperty;
using MediatR;
using System.Security.Claims;

namespace Booking.Api.Features.Properties;

public static class PropertyEndpoints
{
    public static void MapPropertyEndpoints(this WebApplication app)
    {
        app.MapPost("/api/v1/properties",
            async (
                CreatePropertyDto dto,
                HttpContext http,
                ISender sender,
                CancellationToken ct
            ) =>
            {
                var userIdStr =
                    http.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                    http.User.FindFirst("sub")?.Value;

                if (string.IsNullOrWhiteSpace(userIdStr) ||
                    !Guid.TryParse(userIdStr, out var userId))
                {
                    return Results.Unauthorized();
                }

                var command = new CreatePropertyCommand(
                    userId,
                    dto.Name,
                    dto.Description,
                    dto.PropertyType,
                    dto.MaxGuests,
                    dto.CheckInTime,
                    dto.CheckOutTime,
                    dto.Country,
                    dto.City,
                    dto.Street,
                    dto.PostalCode
                );

                var propertyId = await sender.Send(command, ct);

                return Results.Ok(new { propertyId });
            })
        .RequireAuthorization();

        app.MapPut("/api/v1/properties/{id}/approve",
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
                    !Guid.TryParse(userIdStr, out var userId))
                {
                    return Results.Unauthorized();
                }

                var command = new ApprovePropertyCommand(userId, id);
                var propertyId = await sender.Send(command, ct);

                return Results.Ok(new { propertyId });
            })
        .RequireAuthorization();

        app.MapPut("/api/v1/properties/{id}",
            async (
                int id,
                UpdatePropertyDto dto,
                HttpContext http,
                ISender sender,
                CancellationToken ct
            ) =>
            {
                var userIdStr =
                    http.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                    http.User.FindFirst("sub")?.Value;

                if (string.IsNullOrWhiteSpace(userIdStr) ||
                    !Guid.TryParse(userIdStr, out var userId))
                {
                    return Results.Unauthorized();
                }

                var command = new UpdatePropertyCommand(
                    userId,
                    id,
                    dto.Name,
                    dto.Description,
                    dto.Amenities,
                    dto.Rules,
                    dto.PropertyType,
                    dto.MaxGuests,
                    dto.CheckInTime,
                    dto.CheckOutTime,
                    dto.Country,
                    dto.City,
                    dto.Street,
                    dto.PostalCode
                );

                var propertyId = await sender.Send(command, ct);

                return Results.Ok(new { propertyId });
            })
        .RequireAuthorization();

        app.MapPut("/api/v1/properties/{id}/activate",
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
                    !Guid.TryParse(userIdStr, out var userId))
                {
                    return Results.Unauthorized();
                }

                var command = new SetPropertyStatusCommand(
                    userId,
                    id,
                    true
                );

                var propertyId = await sender.Send(command, ct);

                return Results.Ok(new { propertyId });
            })
        .RequireAuthorization();

        app.MapPut("/api/v1/properties/{id}/deactivate",
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
                    !Guid.TryParse(userIdStr, out var userId))
                {
                    return Results.Unauthorized();
                }

                var command = new SetPropertyStatusCommand(
                    userId,
                    id,
                    false
                );

                var propertyId = await sender.Send(command, ct);

                return Results.Ok(new { propertyId });
            })
        .RequireAuthorization();

        app.MapPost("/api/v1/properties/{id}/availability",
            async (
                int id,
                SetAvailabilityDto dto,
                HttpContext http,
                ISender sender,
                CancellationToken ct
            ) =>
            {
                var userIdStr =
                    http.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                    http.User.FindFirst("sub")?.Value;

                if (string.IsNullOrWhiteSpace(userIdStr) ||
                    !Guid.TryParse(userIdStr, out var userId))
                {
                    return Results.Unauthorized();
                }

                var command = new SetAvailabilityCommand(
                    userId,
                    id,
                    dto.Date,
                    dto.Price,
                    dto.IsAvailable
                );

                var propertyId = await sender.Send(command, ct);

                return Results.Ok(new { propertyId });
            })
        .RequireAuthorization();

        app.MapPost("/api/v1/properties/{id}/photos",
            async (
                int id,
                AddPropertyPhotoDto dto,
                HttpContext http,
                ISender sender,
                CancellationToken ct
            ) =>
            {
                var userIdStr =
                    http.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                    http.User.FindFirst("sub")?.Value;

                if (string.IsNullOrWhiteSpace(userIdStr) ||
                    !Guid.TryParse(userIdStr, out var userId))
                {
                    return Results.Unauthorized();
                }

                var command = new AddPropertyPhotoCommand(
                    userId,
                    id,
                    dto.FileName,
                    dto.ContentType,
                    dto.Base64Data,
                    dto.IsPrimary
                );

                var photoId = await sender.Send(command, ct);

                return Results.Ok(new { photoId });
            })
        .RequireAuthorization();

        app.MapGet("/api/v1/properties/{id}/photos",
            async (
                int id,
                ISender sender,
                CancellationToken ct
            ) =>
            {
                var result = await sender.Send(
                    new GetPropertyPhotosQuery(id),
                    ct);

                return Results.Ok(result);
            });

        app.MapGet("/api/v1/properties/search",
            async (
                string city,
                int guests,
                DateOnly date,
                string? propertyType,
                decimal? minPrice,
                decimal? maxPrice,
                ISender sender,
                CancellationToken ct
            ) =>
            {
                var query = new SearchPropertiesQuery(
                    city,
                    guests,
                    date,
                    propertyType,
                    minPrice,
                    maxPrice
                );

                var result = await sender.Send(query, ct);

                return Results.Ok(result);
            });

        app.MapGet("/api/v1/properties",
            async (
                ISender sender,
                CancellationToken ct
            ) =>
            {
                var query = new GetAllPropertiesQuery();
                var result = await sender.Send(query, ct);

                return Results.Ok(result);
            });

        app.MapGet("/api/v1/properties/{id}",
            async (
                int id,
                ISender sender,
                CancellationToken ct
            ) =>
            {
                var query = new GetPropertyByIdQuery(id);
                var result = await sender.Send(query, ct);

                if (result is null)
                    return Results.NotFound();

                return Results.Ok(result);
            });

        app.MapGet("/api/v1/properties/{id}/availability",
            async (
                int id,
                ISender sender,
                CancellationToken ct
            ) =>
            {
                var query = new GetAvailabilityQuery(id);
                var result = await sender.Send(query, ct);

                return Results.Ok(result);
            });
    }
}