using Booking.Application.Features.Properties.Persistence;
using Booking.Domain.Entities.Properties;
using MediatR;

namespace Booking.Application.Features.Properties.GetAvailability;

public class GetAvailabilityQueryHandler
    : IRequestHandler<GetAvailabilityQuery, List<PropertyAvailability>>
{
    private readonly IPropertyRepository _repository;

    public GetAvailabilityQueryHandler(IPropertyRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<PropertyAvailability>> Handle(
        GetAvailabilityQuery request,
        CancellationToken cancellationToken)
    {
        return await _repository.GetAvailabilityByPropertyId(
            request.PropertyId,
            cancellationToken
        );
    }
}