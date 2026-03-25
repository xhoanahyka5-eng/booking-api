using System.Text.Json;
using Booking.Application.Abstractions.Messaging;
using Booking.Application.Common.Events;
using Confluent.Kafka;

namespace Booking.Api.Services.Kafka;

public class KafkaBookingEventProducer : IBookingEventProducer, IDisposable
{
    private readonly IProducer<Null, string> _producer;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KafkaBookingEventProducer> _logger;

    public KafkaBookingEventProducer(
        IConfiguration configuration,
        ILogger<KafkaBookingEventProducer> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092"
        };

        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public async Task PublishAsync(BookingEventMessage message, CancellationToken cancellationToken = default)
    {
        var topic = _configuration["Kafka:BookingEventsTopic"] ?? "booking.events";
        var payload = JsonSerializer.Serialize(message);

        var result = await _producer.ProduceAsync(
            topic,
            new Message<Null, string> { Value = payload },
            CancellationToken.None);

        _logger.LogInformation(
            "Booking event delivered. Topic={Topic}, Offset={Offset}",
            result.Topic,
            result.Offset);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}