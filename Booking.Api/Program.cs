using Booking.Infrastructure;
using Booking.Application;
using Booking.Api.Features.Users;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Diagnostics;

using FluentValidation;

using System.Text;

var builder = WebApplication.CreateBuilder(args);


// ======================
// REGISTER SERVICES
// ======================

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);


// ======================
// JWT AUTHENTICATION
// ======================

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

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

        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey!)
        )
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