using Booking.Application.Common.Exceptions;
using Booking.Application.Features.Properties.Persistence;
using Booking.Application.Features.Users.Persistence;
using Booking.Domain.Entities.Properties;
using MediatR;

namespace Booking.Application.Features.Properties.AddPropertyPhoto;

public class AddPropertyPhotoCommandHandler
    : IRequestHandler<AddPropertyPhotoCommand, int>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IUserRepository _userRepository;

    public AddPropertyPhotoCommandHandler(
        IPropertyRepository propertyRepository,
        IUserRepository userRepository)
    {
        _propertyRepository = propertyRepository;
        _userRepository = userRepository;
    }

    public async Task<int> Handle(
        AddPropertyPhotoCommand request,
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
            throw new UnauthorizedException("You are not allowed to add photos to this property.");

        try
        {
            Convert.FromBase64String(request.Base64Data);
        }
        catch
        {
            throw new ConflictException("Base64Data is not valid.");
        }

        if (request.IsPrimary)
        {
            foreach (var existingPhoto in property.Photos)
            {
                existingPhoto.IsPrimary = false;
            }
        }

        var shouldBePrimary = request.IsPrimary || !property.Photos.Any();

        var photo = new PropertyPhoto
        {
            PropertyId = request.PropertyId,
            FileName = request.FileName,
            ContentType = request.ContentType,
            Base64Data = request.Base64Data,
            IsPrimary = shouldBePrimary
        };

        await _propertyRepository.AddPhotoAsync(photo, cancellationToken);
        await _propertyRepository.SaveChanges(cancellationToken);

        return photo.Id;
    }
}