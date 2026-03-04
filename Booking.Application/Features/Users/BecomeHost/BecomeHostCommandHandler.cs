using Booking.Application.Features.Users.Persistence;
using Booking.Domain.Entities.OwnerProfiles;
using Booking.Domain.Entities.UserRoles;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Features.Users.BecomeHost;

public class BecomeHostCommandHandler
    : IRequestHandler<BecomeHostCommand, string>
{
    private readonly IUserRepository _repo;
    public BecomeHostCommandHandler(IUserRepository repo)
    {
        _repo = repo;
    }


    public async Task<string> Handle(
    BecomeHostCommand request,
    CancellationToken ct)
    {
        return "You are now a Host";
    }
}