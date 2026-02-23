using Booking.Application.Abstractions.Authentication;
using Booking.Application.Features.Users.Persistence;
using Booking.Infrastructure.Authentication;
using Booking.Infrastructure.Data;
using Booking.Infrastructure.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Booking.Infrastructure;

public static class InfrastructureRegistration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 🔹 DbContext
        services.AddDbContext<BookingDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")
            )
        );

        // 🔹 Repositories
        services.AddScoped<IUserRepository, UserRepository>();

        // 🔹 JWT Token Generator
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}