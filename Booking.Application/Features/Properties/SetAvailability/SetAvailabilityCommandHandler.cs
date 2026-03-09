using Booking.Application.Features.Properties.Persistence;
using MediatR;

namespace Booking.Application.Features.Properties.SetAvailability;

public class SetAvailabilityCommandHandler
    : IRequestHandler<SetAvailabilityCommand, int>
{
    private readonly IPropertyRepository _repo;

    public SetAvailabilityCommandHandler(IPropertyRepository repo)
    {
        _repo = repo;
    }

    public async Task<int> Handle(SetAvailabilityCommand request, CancellationToken ct)
    {
        var property = await _repo.GetPropertyById(request.PropertyId, ct);

        if (property == null)
            throw new Exception("Property not found");

        property.AddAvailability(
            request.Date,
            request.Price,
            request.IsAvailable
        );

        await _repo.SaveChanges(ct);

        return property.Id;
    }
}