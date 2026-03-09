using Booking.Application.Features.Properties.Persistence;
using Booking.Domain.Entities.Addresses;
using Booking.Domain.Entities.Properties;
using MediatR;

namespace Booking.Application.Features.Properties.CreateProperty;

public class CreatePropertyCommandHandler
    : IRequestHandler<CreatePropertyCommand, int>
{
    private readonly IPropertyRepository _repo;

    public CreatePropertyCommandHandler(IPropertyRepository repo)
    {
        _repo = repo;
    }

    public async Task<int> Handle(CreatePropertyCommand request, CancellationToken ct)
    {
        // 1️⃣ Krijo Address
        var address = new Address
        {
            Country = request.Country,
            City = request.City,
            Street = request.Street,
            PostalCode = request.PostalCode
        };

        // 2️⃣ Ruaj Address në DB
        await _repo.AddAddress(address, ct);

        // 3️⃣ Konverto PropertyType nga string në enum
        var propertyType = Enum.Parse<PropertyType>(request.PropertyType);

        // 4️⃣ Krijo Property
        var property = new Property(
            request.OwnerId,
            request.Name,
            request.Description,
            propertyType,
            request.MaxGuests,
            request.CheckInTime,
            request.CheckOutTime,
            address.Id
        );

        // 5️⃣ Ruaj Property
        return await _repo.CreateProperty(property, ct);
    }
}