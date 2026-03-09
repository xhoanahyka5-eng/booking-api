using Booking.Domain.Entities.Properties;
using MediatR;

namespace Booking.Application.Features.Properties.GetAllProperties;

public record GetAllPropertiesQuery() : IRequest<List<PropertyDto>>;