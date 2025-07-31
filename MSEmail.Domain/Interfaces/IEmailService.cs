namespace MSEmail.Domain.Interfaces;

/// <summary>
/// Interface para serviço de e-mail
/// </summary>
public interface IEmailService
{
    Task<bool> SendEmailAsync(string recipientEmail, string recipientName, string subject, string body, string? attachmentPath = null, CancellationToken cancellationToken = default);
}
