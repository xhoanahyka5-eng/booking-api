using Booking.Application.Common.Models;
using MediatR;

namespace Booking.Application.Features.Properties.GetAllProperties;

public record GetAllPropertiesQuery(
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PagedResult<PropertyDto>>;