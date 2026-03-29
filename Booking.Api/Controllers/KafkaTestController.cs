using Booking.Application.Abstractions.Logging;
using Booking.Application.Common.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Booking.Api.Controllers;

[ApiController]
[Route("api/kafka-test")]
[AllowAnonymous]
public class KafkaTestController : ControllerBase
{
    private readonly IKafkaLogProducer _kafkaLogProducer;

    public KafkaTestController(IKafkaLogProducer kafkaLogProducer)
    {
        _kafkaLogProducer = kafkaLogProducer;
    }

    [HttpPost("send-log")]
    public async Task<IActionResult> SendLog()
    {
        await _kafkaLogProducer.PublishAsync(
            new LogMessage
            {
                Level = "Information",
                Message = "Test log from KafkaTestController",
                TraceId = Guid.NewGuid().ToString()
            },
            CancellationToken.None);

        return Ok(new { message = "Log sent to Kafka successfully." });
    }
}