using Booking.Application.Abstractions.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Booking.Infrastructure.Email;

public class SendGridEmailService : IEmailService
{
    private readonly SendGridSettings _settings;
    private readonly ILogger<SendGridEmailService> _logger;

    public SendGridEmailService(
        IOptions<SendGridSettings> settings,
        ILogger<SendGridEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        var client = new SendGridClient(_settings.ApiKey);

        var from = new EmailAddress(_settings.FromEmail, _settings.FromName);
        var to = new EmailAddress(message.To);

        var plainTextBody =
            !string.IsNullOrWhiteSpace(message.PlainTextBody)
                ? message.PlainTextBody
                : !string.IsNullOrWhiteSpace(message.Body)
                    ? message.Body
                    : "Please view this email in HTML format.";

        var htmlBody =
            !string.IsNullOrWhiteSpace(message.HtmlBody)
                ? message.HtmlBody
                : !string.IsNullOrWhiteSpace(message.Body)
                    ? $"<pre style=\"font-family:Arial,Helvetica,sans-serif; white-space:pre-wrap;\">{WebUtility.HtmlEncode(message.Body)}</pre>"
                    : $"<pre style=\"font-family:Arial,Helvetica,sans-serif; white-space:pre-wrap;\">{WebUtility.HtmlEncode(plainTextBody)}</pre>";

        var msg = MailHelper.CreateSingleEmail(
            from,
            to,
            message.Subject,
            plainTextBody,
            htmlBody
        );

        var response = await client.SendEmailAsync(msg, cancellationToken);

        if ((int)response.StatusCode >= 200 && (int)response.StatusCode < 300)
        {
            _logger.LogInformation("Email sent successfully to {Email} via SendGrid.", message.To);
            return;
        }

        var responseBody = await response.Body.ReadAsStringAsync(cancellationToken);

        _logger.LogError(
            "Failed to send email to {Email} via SendGrid. StatusCode: {StatusCode}. Response: {Response}",
            message.To,
            response.StatusCode,
            responseBody);

        throw new Exception($"SendGrid email failed with status code {(int)response.StatusCode}.");
    }
}