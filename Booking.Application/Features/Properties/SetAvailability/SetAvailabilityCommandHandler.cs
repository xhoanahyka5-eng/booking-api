using Booking.Application.Common.Exceptions;
using Booking.Application.Features.Properties.Persistence;
using MediatR;

namespace Booking.Application.Features.Properties.SetAvailability;

public class SetAvailabilityCommandHandler
    : IRequestHandler<SetAvailabilityCommand, int>
{
    private readonly IPropertyRepository _repository;

    public SetAvailabilityCommandHandler(IPropertyRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> Handle(
        SetAvailabilityCommand request,
        CancellationToken cancellationToken)
    {
        var property = await _repository.GetPropertyWithAvailabilityAsync(
            request.PropertyId,
            cancellationToken);

        if (property is null)
            throw new NotFoundException("Property not found.");

        if (property.OwnerId != request.OwnerId)
            throw new UnauthorizedException("You are not allowed to modify this property.");

        await _repository.UpsertAvailabilityAsync(
            request.PropertyId,
            request.Date,
            request.Price,
            request.IsAvailable,
            cancellationToken);

        await _repository.SaveChanges(cancellationToken);

        return property.Id;
    }
}