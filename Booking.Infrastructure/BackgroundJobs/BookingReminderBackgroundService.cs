using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Booking.Application.Abstractions.Email;
using Booking.Application.Abstractions.Notifications;
using Booking.Application.Features.Bookings.Persistence;
using Booking.Application.Features.Users.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Booking.Infrastructure.BackgroundJobs;

public sealed class BookingReminderBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingReminderBackgroundService> _logger;
    private readonly object _lock = new();

    private readonly HashSet<string> _sentReminderKeys = new();
    private readonly string _stateFilePath;

    public BookingReminderBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<BookingReminderBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        var appDataFolder = Path.Combine(AppContext.BaseDirectory, "App_Data");
        Directory.CreateDirectory(appDataFolder);

        _stateFilePath = Path.Combine(appDataFolder, "booking-reminder-state.json");
        LoadState();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(30));

        _logger.LogInformation("BookingReminderBackgroundService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRemindersAsync(stoppingToken);
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while sending booking reminders.");
            }
        }

        _logger.LogInformation("BookingReminderBackgroundService stopped.");
    }

    private async Task ProcessRemindersAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var liveNotificationService = scope.ServiceProvider.GetRequiredService<ILiveNotificationService>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1));

        // Check-in reminders
        var upcomingCheckIns = await bookingRepository.GetConfirmedBookingsStartingOnDateAsync(
            tomorrow,
            cancellationToken);

        foreach (var booking in upcomingCheckIns)
        {
            var reminderKey = $"checkin:{booking.Id}";

            if (!TryMarkAsSent(reminderKey))
                continue;

            await SendReminderAsync(
                booking,
                "Booking reminder",
                $"Reminder: your stay at {booking.Property?.Name ?? "your booking"} starts tomorrow ({booking.StartDate:dd/MM/yyyy}).",
                "Your check-in is tomorrow",
                emailService,
                userRepository,
                notificationService,
                liveNotificationService,
                cancellationToken);
        }

        // Check-out reminders
        var upcomingCheckOuts = await bookingRepository.GetConfirmedBookingsEndingOnDateAsync(
            tomorrow,
            cancellationToken);

        foreach (var booking in upcomingCheckOuts)
        {
            var reminderKey = $"checkout:{booking.Id}";

            if (!TryMarkAsSent(reminderKey))
                continue;

            await SendReminderAsync(
                booking,
                "Check-out reminder",
                $"Reminder: your stay at {booking.Property?.Name ?? "your booking"} ends tomorrow ({booking.EndDate:dd/MM/yyyy}).",
                "Your check-out is tomorrow",
                emailService,
                userRepository,
                notificationService,
                liveNotificationService,
                cancellationToken);
        }

        _logger.LogInformation(
            "Processed reminders. Check-ins: {CheckInCount}, Check-outs: {CheckOutCount}.",
            upcomingCheckIns.Count,
            upcomingCheckOuts.Count);
    }

    private async Task SendReminderAsync(
        Booking.Domain.Entities.Bookings.Booking booking,
        string notificationTitle,
        string notificationMessage,
        string emailSubject,
        IEmailService emailService,
        IUserRepository userRepository,
        INotificationService notificationService,
        ILiveNotificationService liveNotificationService,
        CancellationToken cancellationToken)
    {
        var guest = await userRepository.GetByIdAsync(booking.GuestId, cancellationToken);
        var propertyName = booking.Property?.Name ?? "your booking";
        var city = booking.Property?.Address?.City ?? "your destination";
        var startDateText = booking.StartDate.ToString("dd/MM/yyyy");
        var endDateText = booking.EndDate.ToString("dd/MM/yyyy");

        try
        {
            await notificationService.AddAsync(
                booking.GuestId,
                "booking-reminder",
                notificationTitle,
                notificationMessage,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Could not store reminder notification for booking {BookingId}.",
                booking.Id);
        }

        try
        {
            await liveNotificationService.SendToUserAsync(
                booking.GuestId,
                "booking-reminder",
                notificationTitle,
                notificationMessage,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Could not send live reminder notification for booking {BookingId}.",
                booking.Id);
        }

        if (guest is null || string.IsNullOrWhiteSpace(guest.Email))
            return;

        try
        {
            await emailService.SendAsync(
                new EmailMessage
                {
                    To = guest.Email,
                    Subject = emailSubject,
                    PlainTextBody =
$@"Hello {guest.FirstName},

This is a reminder about your upcoming stay.

Property: {propertyName}
City: {city}
Check-in: {startDateText}
Check-out: {endDateText}

Best regards,
Booking Platform Team",

                    HtmlBody =
$@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>{emailSubject}</title>
</head>
<body style=""margin:0; padding:0; background-color:#f4f6f8; font-family:Arial, Helvetica, sans-serif; color:#111827;"">
    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""background-color:#f4f6f8; padding:30px 0;"">
        <tr>
            <td align=""center"">
                <table role=""presentation"" width=""600"" cellspacing=""0"" cellpadding=""0"" style=""background-color:#ffffff; border-radius:14px; overflow:hidden; box-shadow:0 4px 16px rgba(0,0,0,0.08);"">
                    
                    <tr>
                        <td style=""background-color:#2563eb; padding:24px 32px; color:#ffffff;"">
                            <h1 style=""margin:0; font-size:24px; font-weight:700;"">{emailSubject}</h1>
                        </td>
                    </tr>

                    <tr>
                        <td style=""padding:32px;"">
                            <p style=""margin:0 0 16px 0; font-size:16px; line-height:1.6;"">
                                Hello <strong>{guest.FirstName}</strong>,
                            </p>

                            <p style=""margin:0 0 16px 0; font-size:15px; line-height:1.7; color:#374151;"">
                                {notificationMessage}
                            </p>

                            <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""margin:24px 0; background-color:#f9fafb; border:1px solid #e5e7eb; border-radius:10px; padding:16px;"">
                                <tr>
                                    <td style=""padding:8px 0; font-size:14px; color:#111827;"">
                                        <strong>Property:</strong> {propertyName}
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding:8px 0; font-size:14px; color:#111827;"">
                                        <strong>City:</strong> {city}
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding:8px 0; font-size:14px; color:#111827;"">
                                        <strong>Check-in:</strong> {startDateText}
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding:8px 0; font-size:14px; color:#111827;"">
                                        <strong>Check-out:</strong> {endDateText}
                                    </td>
                                </tr>
                            </table>

                            <p style=""margin:0; font-size:15px; line-height:1.7; color:#374151;"">
                                If you need any help, we are here for you.
                            </p>
                        </td>
                    </tr>

                    <tr>
                        <td style=""padding:20px 32px; background-color:#f9fafb; border-top:1px solid #e5e7eb; font-size:13px; color:#6b7280;"">
                            Best regards,<br />
                            <strong>Booking Platform Team</strong>
                        </td>
                    </tr>

                </table>
            </td>
        </tr>
    </table>
</body>
</html>"
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Could not send reminder email for booking {BookingId} to {Email}.",
                booking.Id,
                guest.Email);
        }
    }

    private bool TryMarkAsSent(string key)
    {
        lock (_lock)
        {
            if (_sentReminderKeys.Contains(key))
                return false;

            _sentReminderKeys.Add(key);
            SaveState();
            return true;
        }
    }

    private void LoadState()
    {
        try
        {
            if (!File.Exists(_stateFilePath))
                return;

            var json = File.ReadAllText(_stateFilePath);
            var items = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();

            lock (_lock)
            {
                _sentReminderKeys.Clear();
                foreach (var item in items)
                {
                    _sentReminderKeys.Add(item);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load booking reminder state.");
        }
    }

    private void SaveState()
    {
        try
        {
            var items = _sentReminderKeys.ToList();
            var json = JsonSerializer.Serialize(items, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_stateFilePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not save booking reminder state.");
        }
    }
}