using Booking.Application.Common.Exceptions;
using Booking.Application.Features.Properties.Persistence;
using Booking.Domain.Entities.Properties;
using MediatR;

namespace Booking.Application.Features.Properties.UpdateProperty;

public class UpdatePropertyCommandHandler
    : IRequestHandler<UpdatePropertyCommand, int>
{
    private readonly IPropertyRepository _repository;

    public UpdatePropertyCommandHandler(IPropertyRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> Handle(
        UpdatePropertyCommand request,
        CancellationToken cancellationToken)
    {
        var property = await _repository.GetPropertyById(request.PropertyId, cancellationToken);

        if (property is null)
            throw new NotFoundException("Property not found.");

        if (property.OwnerId != request.OwnerId)
            throw new UnauthorizedException("You are not allowed to update this property.");

        var propertyTypeText = Normalize(request.PropertyType);

        if (!Enum.TryParse<PropertyType>(propertyTypeText, true, out var propertyType))
            throw new ConflictException("Invalid property type.");

        var country = Normalize(request.Country);
        var city = Normalize(request.City);
        var street = Normalize(request.Street);
        var postalCode = Normalize(request.PostalCode);

        var address = await _repository.GetOrCreateAddressAsync(
            country,
            city,
            street,
            postalCode,
            cancellationToken);

        property.UpdateDetails(
            Normalize(request.Name),
            NormalizeNullable(request.Description),
            request.MaxGuests,
            request.CheckInTime,
            request.CheckOutTime
        );

        property.UpdateAmenitiesAndRules(
            NormalizeNullable(request.Amenities),
            NormalizeNullable(request.Rules)
        );

        property.ChangePropertyType(propertyType);
        property.ChangeAddress(address.Id);

        await _repository.SaveChanges(cancellationToken);

        return property.Id;
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim();
    }

    private static string? NormalizeNullable(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}