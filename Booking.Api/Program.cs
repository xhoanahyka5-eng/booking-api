using Booking.Api.Features.Users;
using Booking.Api.Middleware;
using Booking.Application;
using Booking.Infrastructure;

using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication() // ✅ KJO MUNGON
    .ConfigurePersistence(builder.Configuration)
    .ConfigureJWT(builder.Configuration);

var app = builder.Build();

app.UseCustomMiddlewares();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/test-error", () =>
{
    throw new Exception("Middleware test");
});

app.MapUserEndpoints();


app.Run();