using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Booking.Application.Abstractions.Notifications;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Booking.Infrastructure.Notifications;

public sealed class FileNotificationService : INotificationService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _filePath;
    private readonly SemaphoreSlim _mutex = new(1, 1);
    private readonly ILogger<FileNotificationService> _logger;

    public FileNotificationService(
        IHostEnvironment hostEnvironment,
        ILogger<FileNotificationService> logger)
    {
        _logger = logger;

        var appDataDirectory = Path.Combine(hostEnvironment.ContentRootPath, "App_Data");
        Directory.CreateDirectory(appDataDirectory);

        _filePath = Path.Combine(appDataDirectory, "notifications.json");

        if (!File.Exists(_filePath))
        {
            File.WriteAllText(_filePath, "[]");
        }
    }

    public async Task AddAsync(
        Guid userId,
        string type,
        string title,
        string message,
        CancellationToken cancellationToken = default)
    {
        await _mutex.WaitAsync(cancellationToken);
        try
        {
            var items = await ReadNotificationsAsync(cancellationToken);
            items.Add(new StoredNotification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = type,
                Title = title,
                Message = message,
                CreatedAtUtc = DateTime.UtcNow
            });

            await using var writeStream = File.Create(_filePath);
            await JsonSerializer.SerializeAsync(writeStream, items, JsonOptions, cancellationToken);
        }
        finally
        {
            _mutex.Release();
        }

        _logger.LogInformation(
            "Notification stored for user {UserId}. Type={Type}, Title={Title}",
            userId,
            type,
            title);
    }

    private async Task<List<StoredNotification>> ReadNotificationsAsync(CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(_filePath);
        var items = await JsonSerializer.DeserializeAsync<List<StoredNotification>>(stream, cancellationToken: cancellationToken);
        return items ?? [];
    }

    private sealed class StoredNotification
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
    }
}
