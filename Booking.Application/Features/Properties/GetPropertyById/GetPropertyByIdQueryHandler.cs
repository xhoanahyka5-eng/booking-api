using AutoMapper;
using Booking.Application.Features.Properties.GetAllProperties;
using Booking.Application.Features.Properties.Persistence;
using MediatR;

namespace Booking.Application.Features.Properties.GetPropertyById;

public class GetPropertyByIdQueryHandler
    : IRequestHandler<GetPropertyByIdQuery, PropertyDto?>
{
    private readonly IPropertyRepository _repo;
    private readonly IMapper _mapper;

    public GetPropertyByIdQueryHandler(
        IPropertyRepository repo,
        IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<PropertyDto?> Handle(
        GetPropertyByIdQuery request,
        CancellationToken ct)
    {
        var property = await _repo.GetPropertyById(request.Id, ct);

        if (property == null)
            return null;

        return _mapper.Map<PropertyDto>(property);
    }
}