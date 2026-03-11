using Booking.Application.Common.Exceptions;
using Booking.Application.Features.Properties.Persistence;
using Booking.Application.Features.Users.Persistence;
using MediatR;

namespace Booking.Application.Features.Properties.SetPropertyStatus;

public class SetPropertyStatusCommandHandler
    : IRequestHandler<SetPropertyStatusCommand, int>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IUserRepository _userRepository;

    public SetPropertyStatusCommandHandler(
        IPropertyRepository propertyRepository,
        IUserRepository userRepository)
    {
        _propertyRepository = propertyRepository;
        _userRepository = userRepository;
    }

    public async Task<int> Handle(
        SetPropertyStatusCommand request,
        CancellationToken cancellationToken)
    {
        var property = await _propertyRepository.GetPropertyById(
            request.PropertyId,
            cancellationToken);

        if (property is null)
            throw new NotFoundException("Property not found.");

        var actor = await _userRepository.GetByIdWithRolesAsync(
            request.ActorUserId,
            cancellationToken);

        if (actor is null)
            throw new NotFoundException("User not found.");

        var isAdmin = actor.UserRoles
            .Any(ur => ur.Role != null && ur.Role.Name == "Admin");

        var isOwner = property.OwnerId == request.ActorUserId;

        if (!isAdmin && !isOwner)
            throw new UnauthorizedException("You are not allowed to change this property status.");

        property.SetActive(request.IsActive);

        await _propertyRepository.SaveChanges(cancellationToken);

        return property.Id;
    }
}