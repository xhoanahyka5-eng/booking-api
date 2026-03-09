using MediatR;

namespace Booking.Application.Features.Properties.Users.GetAllUsers;

public record GetAllUsersQuery() : IRequest<List<UserDto>>;