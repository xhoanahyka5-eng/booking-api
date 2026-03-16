namespace Booking.Application.Abstractions.Email;

public interface IEmailService
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken);
}
