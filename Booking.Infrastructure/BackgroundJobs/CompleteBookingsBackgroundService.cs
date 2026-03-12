using Booking.Application.Features.Bookings.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Booking.Infrastructure.BackgroundJobs;

public class CompleteBookingsBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CompleteBookingsBackgroundService> _logger;

    public CompleteBookingsBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<CompleteBookingsBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

        _logger.LogInformation("CompleteBookingsBackgroundService started.");

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
                _logger.LogError(ex, "Error while completing expired bookings.");
            }
        }

        _logger.LogInformation("CompleteBookingsBackgroundService stopped.");
    }

    private async Task ProcessBookingsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var bookingsToComplete = await bookingRepository.GetConfirmedBookingsToCompleteAsync(
            today,
            cancellationToken);

        if (bookingsToComplete.Count == 0)
            return;

        foreach (var booking in bookingsToComplete)
        {
            booking.Complete();
        }

        await bookingRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Completed {Count} booking(s) automatically.",
            bookingsToComplete.Count);
    }
}