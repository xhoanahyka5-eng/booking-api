using Booking.Application.Features.Properties.CreateProperty;
using Booking.Application.Features.Properties.GetAllProperties;
using Booking.Application.Features.Properties.GetAvailability;
using Booking.Application.Features.Properties.GetPropertyById;
using Booking.Application.Features.Properties.SearchProperties;
using Booking.Application.Features.Properties.SetAvailability;
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
                ISender sender
            ) =>
            {
                var userIdStr = http.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                    return Results.Unauthorized();

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

                var propertyId = await sender.Send(command);

                return Results.Ok(new { propertyId });
            })
        .RequireAuthorization();

        app.MapPost("/api/v1/properties/{id}/availability",
            async (
                int id,
                SetAvailabilityDto dto,
                ISender sender
            ) =>
            {
                var command = new SetAvailabilityCommand(
                    id,
                    dto.Date,
                    dto.Price,
                    dto.IsAvailable
                );

                var propertyId = await sender.Send(command);

                return Results.Ok(new { propertyId });
            })
        .RequireAuthorization();

        app.MapGet("/api/v1/properties/search",
            async (
                string city,
                int guests,
                DateOnly date,
                ISender sender
            ) =>
            {
                var query = new SearchPropertiesQuery(city, guests, date);

                var result = await sender.Send(query);

                return Results.Ok(result);
            });

        app.MapGet("/api/v1/properties",
            async (ISender sender) =>
            {
                var query = new GetAllPropertiesQuery();

                var result = await sender.Send(query);

                return Results.Ok(result);
            });

        app.MapGet("/api/v1/properties/{id}",
            async (int id, ISender sender) =>
            {
                var query = new GetPropertyByIdQuery(id);

                var result = await sender.Send(query);

                if (result == null)
                    return Results.NotFound();

                return Results.Ok(result);
            });

        app.MapGet("/api/v1/properties/{id}/availability",
            async (int id, ISender sender) =>
            {
                var query = new GetAvailabilityQuery(id);

                var result = await sender.Send(query);

                return Results.Ok(result);
            });
    }
}