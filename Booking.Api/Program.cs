using Booking.Api.Features.Bookings;
using Booking.Api.Features.Properties;
using Booking.Api.Features.Users;
using Booking.Api.Middleware;
using Booking.Application;
using Booking.Application.Abstractions.Logging;
using Booking.Infrastructure;
using Booking.Infrastructure.Data;
using Booking.Api.Features.Reviews;
using Booking.Api.Services.Kafka;
using Booking.Infrastructure.SignalR;
using Booking.Application.Abstractions.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .ConfigurePersistence(builder.Configuration)
    .ConfigureJWT(builder.Configuration);

builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddSingleton<IKafkaLogProducer, KafkaLogProducer>();
builder.Services.AddSingleton<IBookingEventProducer, KafkaBookingEventProducer>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
    await DbSeeder.SeedAsync(db);
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<NotificationHub>("/hubs/notifications");

app.MapUserEndpoints();
app.MapPropertyEndpoints();
app.MapBookingEndpoints();
app.MapReviewEndpoints();
app.MapControllers();

app.Run();