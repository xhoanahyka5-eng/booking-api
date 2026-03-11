using Booking.Application.Common.Exceptions;
using Booking.Application.Features.Properties.Persistence;
using Booking.Application.Features.Users.Persistence;
using MediatR;

namespace Booking.Application.Features.Properties.ApproveProperty;

public class ApprovePropertyCommandHandler
    : IRequestHandler<ApprovePropertyCommand, int>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IUserRepository _userRepository;

    public ApprovePropertyCommandHandler(
        IPropertyRepository propertyRepository,
        IUserRepository userRepository)
    {
        _propertyRepository = propertyRepository;
        _userRepository = userRepository;
    }

    public async Task<int> Handle(
        ApprovePropertyCommand request,
        CancellationToken cancellationToken)
    {
        var adminUser = await _userRepository.GetByIdWithRolesAsync(
            request.AdminId,
            cancellationToken);

        if (adminUser is null)
            throw new NotFoundException("User not found.");

        var isAdmin = adminUser.UserRoles
            .Any(ur => ur.Role != null && ur.Role.Name == "Admin");

        if (!isAdmin)
            throw new UnauthorizedException("Only admin can approve properties.");

        var property = await _propertyRepository.GetPropertyById(
            request.PropertyId,
            cancellationToken);

        if (property is null)
            throw new NotFoundException("Property not found.");

        if (property.IsApproved)
            throw new ConflictException("Property is already approved.");

        property.Approve();

        await _propertyRepository.SaveChanges(cancellationToken);

        return property.Id;
    }
}