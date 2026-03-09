using AutoMapper;
using Booking.Application.Features.Properties.Persistence;
using MediatR;

namespace Booking.Application.Features.Properties.GetAllProperties;

public class GetAllPropertiesQueryHandler
    : IRequestHandler<GetAllPropertiesQuery, List<PropertyDto>>
{
    private readonly IPropertyRepository _repo;
    private readonly IMapper _mapper;

    public GetAllPropertiesQueryHandler(
        IPropertyRepository repo,
        IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<List<PropertyDto>> Handle(
        GetAllPropertiesQuery request,
        CancellationToken ct)
    {
        var properties = await _repo.GetAllAsync(ct);

        return _mapper.Map<List<PropertyDto>>(properties);
    }
}