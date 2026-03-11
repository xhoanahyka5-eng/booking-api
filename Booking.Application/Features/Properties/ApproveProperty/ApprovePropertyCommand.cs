using MediatR;

namespace Booking.Application.Features.Properties.ApproveProperty;

public record ApprovePropertyCommand(
    Guid AdminId,
    int PropertyId
) : IRequest<int>;