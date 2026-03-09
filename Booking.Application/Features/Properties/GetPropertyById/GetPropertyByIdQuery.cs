using MediatR;
using Booking.Application.Features.Properties.GetAllProperties;

namespace Booking.Application.Features.Properties.GetPropertyById;

public record GetPropertyByIdQuery(int Id) : IRequest<PropertyDto?>;