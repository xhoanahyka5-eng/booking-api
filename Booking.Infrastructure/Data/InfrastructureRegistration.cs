using Microsoft.EntityFrameworkCore;
using Booking.Application.Abstractions.Authentication;
using Booking.Application.Features.Users.Persistence;
using Booking.Infrastructure.Authentication;
using Booking.Infrastructure.Data;
using Booking.Infrastructure.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

public static class InfrastructureRegistration
{
    public static IServiceCollection ConfigurePersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<BookingDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")
            )
        );

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }

    public static IServiceCollection ConfigureJWT(
     this IServiceCollection services,
     IConfiguration configuration)
    {
        services.Configure<JwtSettings>(
            configuration.GetSection("JwtSettings")
        );

        var jwtSettings = configuration
            .GetSection("JwtSettings")
            .Get<JwtSettings>();

        if (jwtSettings is null || string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
            throw new Exception("JWT configuration is missing or invalid.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme =
                JwtBearerDefaults.AuthenticationScheme;

            options.DefaultChallengeScheme =
                JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,

                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings.SecretKey)
                        ),

                    ClockSkew = TimeSpan.Zero
                };
        });

        services.AddAuthorization();

        return services;
    }
}