using Booking.Api.Features.Bookings;
using Booking.Api.Features.Properties;
using Booking.Api.Features.Users;
using Booking.Api.Middleware;
using Booking.Application;
using Booking.Infrastructure;
using Booking.Infrastructure.Data;
using Booking.Api.Features.Reviews;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .ConfigurePersistence(builder.Configuration)
    .ConfigureJWT(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
    await DbSeeder.SeedAsync(db);
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapUserEndpoints();
app.MapPropertyEndpoints();
app.MapBookingEndpoints();
app.MapReviewEndpoints();

app.MapGet("/test-error", () =>
{
    throw new Exception("Middleware test");
});

app.Run();