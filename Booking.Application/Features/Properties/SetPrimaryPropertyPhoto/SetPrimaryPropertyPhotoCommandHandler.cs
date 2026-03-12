using Booking.Application.Common.Exceptions;
using Booking.Application.Features.Properties.Persistence;
using Booking.Application.Features.Users.Persistence;
using MediatR;

namespace Booking.Application.Features.Properties.SetPrimaryPropertyPhoto;

public class SetPrimaryPropertyPhotoCommandHandler
    : IRequestHandler<SetPrimaryPropertyPhotoCommand, int>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IUserRepository _userRepository;

    public SetPrimaryPropertyPhotoCommandHandler(
        IPropertyRepository propertyRepository,
        IUserRepository userRepository)
    {
        _propertyRepository = propertyRepository;
        _userRepository = userRepository;
    }

    public async Task<int> Handle(
        SetPrimaryPropertyPhotoCommand request,
        CancellationToken cancellationToken)
    {
        var property = await _propertyRepository.GetPropertyWithPhotosAsync(
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
            throw new UnauthorizedException("You are not allowed to update photos for this property.");

        var photo = property.Photos.FirstOrDefault(p => p.Id == request.PhotoId);

        if (photo is null)
            throw new NotFoundException("Photo not found.");

        foreach (var existingPhoto in property.Photos)
        {
            existingPhoto.IsPrimary = false;
        }

        photo.IsPrimary = true;

        await _propertyRepository.SaveChanges(cancellationToken);

        return photo.Id;
    }
}