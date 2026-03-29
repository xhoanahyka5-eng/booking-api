using System.Text.Json;
using Booking.Application.Abstractions.Logging;
using Confluent.Kafka;
using AppLogMessage = Booking.Application.Common.Logging.LogMessage;

namespace Booking.Api.Services.Kafka;

public sealed class KafkaLogProducer : IKafkaLogProducer, IDisposable
{
    private readonly IProducer<Null, string> _producer;
    private readonly ILogger<KafkaLogProducer> _logger;
    private readonly string _topic;

    public KafkaLogProducer(
        IConfiguration configuration,
        ILogger<KafkaLogProducer> logger)
    {
        _logger = logger;

        var bootstrapServers =
            configuration.GetValue<string>("Kafka:BootstrapServers")
            ?? "localhost:9092";
        _topic = configuration.GetValue<string>("Kafka:Topic") ?? "booking.logs";

        _logger.LogInformation(
            "Kafka log producer configured. BootstrapServers={BootstrapServers}, Topic={Topic}",
            bootstrapServers,
            _topic);

        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            ClientId = "booking-api",
            SecurityProtocol = SecurityProtocol.Plaintext,
            MessageTimeoutMs = 5000,
            SocketTimeoutMs = 5000,
            RequestTimeoutMs = 5000
        };

        _producer = new ProducerBuilder<Null, string>(config)
            .SetErrorHandler((_, e) =>
            {
                _logger.LogError("Kafka producer error: {Reason}", e.Reason);
            })
            .Build();
    }

    public async Task PublishAsync(AppLogMessage logMessage, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(logMessage);

        var result = await _producer.ProduceAsync(
            _topic,
            new Message<Null, string> { Value = payload },
            CancellationToken.None);

        _logger.LogInformation(
            "Kafka delivered. Topic={Topic}, Partition={Partition}, Offset={Offset}",
            result.Topic,
            result.Partition,
            result.Offset);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}