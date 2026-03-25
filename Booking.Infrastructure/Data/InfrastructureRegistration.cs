using Booking.Application.Abstractions.Authentication;
using Booking.Application.Abstractions.Email;
using Booking.Application.Features.Bookings.Persistence;
using Booking.Application.Features.Properties.Persistence;
using Booking.Application.Features.Reviews.Persistence;
using Booking.Application.Features.Users.Persistence;
using Booking.Infrastructure.Authentication;
using Booking.Infrastructure.BackgroundJobs;
using Booking.Infrastructure.Data;
using Booking.Infrastructure.Email;
using Booking.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Booking.Infrastructure;

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

        services.Configure<SendGridSettings>(
            configuration.GetSection("SendGridSettings"));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPropertyRepository, PropertyRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IEmailService, SendGridEmailService>();

        services.AddHostedService<CompleteBookingsBackgroundService>();
        services.AddHostedService<ExpirePendingBookingsBackgroundService>();
        services.AddHostedService<BookingReminderBackgroundService>();
        services.AddScoped<ILiveNotificationService, SignalRLiveNotificationService>();

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
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),

                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;

                    Console.WriteLine($"JWT PATH: {path}");
                    Console.WriteLine($"JWT TOKEN EXISTS: {!string.IsNullOrEmpty(accessToken)}");

                    if (!string.IsNullOrEmpty(accessToken) &&
                        path.StartsWithSegments("/hubs/notifications"))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                },

                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine("JWT AUTH FAILED: " + context.Exception.Message);
                    return Task.CompletedTask;
                }
        };
        });

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddAuthorization();

        return services;
    }
}