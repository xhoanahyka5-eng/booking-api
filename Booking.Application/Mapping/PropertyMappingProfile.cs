using AutoMapper;
using Booking.Domain.Entities.Properties;
using Booking.Application.Features.Properties;

namespace Booking.Application.Mapping;

public class PropertyMappingProfile : Profile
{
    public PropertyMappingProfile()
    {
        CreateMap<Property, PropertyDto>()
            .ForMember(
                d => d.City,
                opt => opt.MapFrom(src => src.Address.City));
    }
}