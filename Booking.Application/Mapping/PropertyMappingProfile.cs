using AutoMapper;
using Booking.Application.Features.Properties;
using Booking.Domain.Entities.Properties;

namespace Booking.Application.Mapping;

public class PropertyMappingProfile : Profile
{
    public PropertyMappingProfile()
    {
        CreateMap<Property, PropertyDto>()
            .ForMember(d => d.PropertyType,
                opt => opt.MapFrom(src => src.PropertyType.ToString()))
            .ForMember(d => d.Country,
                opt => opt.MapFrom(src => src.Address.Country))
            .ForMember(d => d.City,
                opt => opt.MapFrom(src => src.Address.City))
            .ForMember(d => d.Street,
                opt => opt.MapFrom(src => src.Address.Street))
            .ForMember(d => d.PostalCode,
                opt => opt.MapFrom(src => src.Address.PostalCode));
    }
}