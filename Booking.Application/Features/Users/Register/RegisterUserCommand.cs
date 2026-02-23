using MediatR;
using Booking.Domain.Entities.Users;   // ✅ KJO ËSHTË E SAKTË

namespace Booking.Application.Features.Users.Register;

public record RegisterUserCommand(CreateUserDto UserDto) : IRequest<Guid>;