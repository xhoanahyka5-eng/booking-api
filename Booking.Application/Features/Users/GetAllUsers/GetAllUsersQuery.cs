using MediatR;

namespace Booking.Application.Features.Users.GetAllUsers;

public record GetAllUsersQuery(Guid AdminId) : IRequest<List<UserDto>>;