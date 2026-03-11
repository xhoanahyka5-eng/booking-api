using Booking.Application.Features.Properties.Persistence;
using MediatR;

namespace Booking.Application.Features.Properties.GetAvailability;

public class GetAvailabilityQueryHandler
    : IRequestHandler<GetAvailabilityQuery, List<AvailabilityDto>>
{
    private readonly IPropertyRepository _repository;

    public GetAvailabilityQueryHandler(IPropertyRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<AvailabilityDto>> Handle(
        GetAvailabilityQuery request,
        CancellationToken cancellationToken)
    {
        var availability = await _repository.GetAvailabilityByPropertyId(
            request.PropertyId,
            cancellationToken
        );

        return availability
            .Select(a => new AvailabilityDto
            {
                Date = a.Date,
                Price = a.Price,
                IsAvailable = a.IsAvailable
            })
            .ToList();
    }
}