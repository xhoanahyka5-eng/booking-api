using Booking.Application.Features.Bookings.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Booking.Infrastructure.BackgroundJobs;

public class ExpirePendingBookingsBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpirePendingBookingsBackgroundService> _logger;

    private const int PendingExpirationHours = 24;

    public ExpirePendingBookingsBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<ExpirePendingBookingsBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

        _logger.LogInformation("ExpirePendingBookingsBackgroundService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBookingsAsync(stoppingToken);
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while expiring pending bookings.");
            }
        }

        _logger.LogInformation("ExpirePendingBookingsBackgroundService stopped.");
    }

    private async Task ProcessBookingsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        var cutoffUtc = DateTime.UtcNow.AddHours(-PendingExpirationHours);

        var bookingsToExpire = await bookingRepository.GetPendingBookingsToExpireAsync(
            cutoffUtc,
            cancellationToken);

        if (bookingsToExpire.Count == 0)
            return;

        foreach (var booking in bookingsToExpire)
        {
            booking.Expire();
        }

        await bookingRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Expired {Count} pending booking(s) automatically.",
            bookingsToExpire.Count);
    }
}