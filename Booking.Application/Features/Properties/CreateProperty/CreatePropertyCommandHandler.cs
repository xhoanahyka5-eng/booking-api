using Booking.Application.Common.Exceptions;
using Booking.Application.Features.Properties.Persistence;
using Booking.Application.Features.Users.Persistence;
using Booking.Domain.Entities.Properties;
using MediatR;

namespace Booking.Application.Features.Properties.CreateProperty;

public class CreatePropertyCommandHandler
    : IRequestHandler<CreatePropertyCommand, int>
{
    private readonly IPropertyRepository _repo;
    private readonly IUserRepository _userRepository;

    public CreatePropertyCommandHandler(
        IPropertyRepository repo,
        IUserRepository userRepository)
    {
        _repo = repo;
        _userRepository = userRepository;
    }

    public async Task<int> Handle(CreatePropertyCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(request.OwnerId, ct);

        if (user is null)
            throw new NotFoundException("User not found.");

        var isHostOrAdmin = user.UserRoles.Any(ur =>
            ur.Role != null &&
            (ur.Role.Name == "Host" || ur.Role.Name == "Admin"));

        if (!isHostOrAdmin)
            throw new UnauthorizedException("Only Host or Admin can create properties.");

        var propertyTypeText = Normalize(request.PropertyType);

        if (!Enum.TryParse<PropertyType>(propertyTypeText, true, out var propertyType))
            throw new ConflictException("Invalid property type.");

        var country = Normalize(request.Country);
        var city = Normalize(request.City);
        var street = Normalize(request.Street);
        var postalCode = Normalize(request.PostalCode);

        var address = await _repo.GetOrCreateAddressAsync(
            country,
            city,
            street,
            postalCode,
            ct);

        var property = new Property(
            request.OwnerId,
            Normalize(request.Name),
            NormalizeNullable(request.Description),
            propertyType,
            request.MaxGuests,
            request.CheckInTime,
            request.CheckOutTime,
            address.Id
        );

        return await _repo.CreateProperty(property, ct);
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