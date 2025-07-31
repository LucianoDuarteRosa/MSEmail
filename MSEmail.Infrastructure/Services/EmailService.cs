using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MSEmail.Domain.Interfaces;
using MSEmail.Domain.Models;

namespace MSEmail.Infrastructure.Services;

/// <summary>
/// Serviço de e-mail usando MailKit
/// </summary>
public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<SmtpSettings> smtpSettings, ILogger<EmailService> logger)
    {
        _smtpSettings = smtpSettings.Value;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string recipientEmail, string recipientName, string subject, string body, string? attachmentPath = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Enviando e-mail para: {Email}", recipientEmail);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpSettings.FromName, _smtpSettings.FromEmail));
            message.To.Add(new MailboxAddress(recipientName, recipientEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body
            };

            // Adicionar anexo PDF se especificado
            if (!string.IsNullOrEmpty(attachmentPath) && File.Exists(attachmentPath))
            {
                bodyBuilder.Attachments.Add(attachmentPath);
                _logger.LogInformation("Anexo adicionado: {AttachmentPath}", attachmentPath);
            }

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            // Configurar timeout
            client.Timeout = _smtpSettings.TimeoutInSeconds * 1000;

            // Conectar ao servidor SMTP
            await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port,
                _smtpSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
                cancellationToken);

            // Autenticar se credenciais estão configuradas
            if (!string.IsNullOrEmpty(_smtpSettings.Username) && !string.IsNullOrEmpty(_smtpSettings.Password))
            {
                await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password, cancellationToken);
            }

            // Enviar e-mail
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("E-mail enviado com sucesso para: {Email}", recipientEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar e-mail para: {Email}", recipientEmail);
            return false;
        }
    }
}
