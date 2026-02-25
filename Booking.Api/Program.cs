using Booking.Api.Features.Users;
using Booking.Application;
using Booking.Application.Abstractions.Authentication;
using Booking.Infrastructure;

using FluentValidation;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ======================
// REGISTER SERVICES
// ======================

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ✅ REGISTER FLUENT VALIDATORS
builder.Services.AddValidatorsFromAssemblyContaining<
    Booking.Application.Features.Users.Login.LoginUserCommandValidation>();

// ======================
// JWT SETTINGS (Strongly Typed)
// ======================

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

var jwtSettings = builder.Configuration
    .GetSection("JwtSettings")
    .Get<JwtSettings>();

if (jwtSettings is null || string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
{
    throw new Exception("JWT configuration is missing or invalid.");
}

// ======================
// JWT AUTHENTICATION
// ======================

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.SecretKey)
        ),

        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// ======================
// GLOBAL EXCEPTION HANDLING
// ======================

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.ContentType = "application/json";

        var exception = context.Features
            .Get<IExceptionHandlerFeature>()?
            .Error;

        if (exception is ValidationException validationException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                errors = validationException.Errors
                    .Select(e => e.ErrorMessage)
            });
            return;
        }

        if (exception is UnauthorizedAccessException)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                message = exception.Message
            });
            return;
        }

        if (exception is InvalidOperationException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                message = exception.Message
            });
            return;
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new
        {
            message = "Internal Server Error"
        });
    });
});

// ======================
// MIDDLEWARE
// ======================

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// ======================
// MAP ENDPOINTS
// ======================

app.MapUserEndpoints();

app.Run();