using Microsoft.Extensions.Options;
using MSEmail.Domain.Entities;
using MSEmail.Domain.Interfaces;
using MSEmail.Domain.Models;

namespace MSEmail.Worker;

/// <summary>
/// Worker responsável por processar a fila de e-mails
/// </summary>
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMessageQueueService _messageQueueService;
    private readonly RabbitMqSettings _rabbitMqSettings;

    public Worker(
        ILogger<Worker> logger,
        IServiceProvider serviceProvider,
        IMessageQueueService messageQueueService,
        IOptions<RabbitMqSettings> rabbitMqSettings)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _messageQueueService = messageQueueService;
        _rabbitMqSettings = rabbitMqSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker de processamento de e-mails iniciado em: {time}", DateTimeOffset.Now);

        try
        {
            // Iniciar consumo da fila
            await _messageQueueService.ConsumeEmailAsync<EmailQueueMessage>(ProcessEmailMessage, stoppingToken);

            // Aguardar até que o serviço seja interrompido
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Worker de processamento de e-mails foi cancelado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no worker de processamento de e-mails");
            throw;
        }
    }

    /// <summary>
    /// Processa uma mensagem de e-mail da fila
    /// </summary>
    /// <param name="message">Mensagem de e-mail</param>
    private async Task ProcessEmailMessage(EmailQueueMessage message)
    {
        using var scope = _serviceProvider.CreateScope();
        var emailLogRepository = scope.ServiceProvider.GetRequiredService<IEmailLogRepository>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var fileStorageService = scope.ServiceProvider.GetRequiredService<IFileStorageService>();

        _logger.LogInformation("Processando e-mail para: {Email} (EmailLogId: {EmailLogId})",
            message.RecipientEmail, message.EmailLogId);

        try
        {
            // Buscar o log de e-mail
            var emailLog = await emailLogRepository.GetByIdAsync(message.EmailLogId);
            if (emailLog == null)
            {
                _logger.LogWarning("EmailLog com ID {EmailLogId} não encontrado", message.EmailLogId);
                return;
            }

            // Marcar como processando
            emailLog.Status = EmailStatus.Processing;
            emailLog.IncrementAttempt();
            await emailLogRepository.UpdateAsync(emailLog);

            // Preparar caminho do anexo PDF se especificado
            string? pdfPath = null;
            if (!string.IsNullOrEmpty(message.PdfPath))
            {
                if (await fileStorageService.FileExistsAsync(message.PdfPath))
                {
                    // Para armazenamento local, usar o caminho diretamente
                    pdfPath = message.PdfPath;
                }
                else
                {
                    _logger.LogWarning("Arquivo PDF não encontrado: {PdfPath}", message.PdfPath);
                }
            }

            // Enviar e-mail
            var success = await emailService.SendEmailAsync(
                message.RecipientEmail,
                message.RecipientName,
                message.Subject,
                message.Body,
                pdfPath);

            if (success)
            {
                // Marcar como enviado
                emailLog.MarkAsSent();
                await emailLogRepository.UpdateAsync(emailLog);

                _logger.LogInformation("E-mail enviado com sucesso para: {Email}", message.RecipientEmail);
            }
            else
            {
                // Marcar como falha
                await HandleEmailFailure(emailLog, emailLogRepository, "Falha no envio do e-mail");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar e-mail para: {Email}", message.RecipientEmail);

            try
            {
                var emailLog = await emailLogRepository.GetByIdAsync(message.EmailLogId);
                if (emailLog != null)
                {
                    await HandleEmailFailure(emailLog, emailLogRepository, ex.Message);
                }
            }
            catch (Exception updateEx)
            {
                _logger.LogError(updateEx, "Erro ao atualizar status de falha para EmailLog: {EmailLogId}", message.EmailLogId);
            }
        }
    }

    /// <summary>
    /// Trata falhas no envio de e-mail
    /// </summary>
    /// <param name="emailLog">Log do e-mail</param>
    /// <param name="repository">Repositório de logs</param>
    /// <param name="errorMessage">Mensagem de erro</param>
    private async Task HandleEmailFailure(EmailLog emailLog, IEmailLogRepository repository, string errorMessage)
    {
        if (emailLog.ShouldRetry())
        {
            // Marcar para nova tentativa
            emailLog.Status = EmailStatus.Retrying;
            emailLog.ErrorMessage = $"Tentativa {emailLog.AttemptCount}/{emailLog.MaxAttempts}: {errorMessage}";
            await repository.UpdateAsync(emailLog);

            _logger.LogWarning("E-mail marcado para nova tentativa: {EmailLogId} (Tentativa {Attempt}/{Max})",
                emailLog.Id, emailLog.AttemptCount, emailLog.MaxAttempts);

            // Reagendar para nova tentativa após delay
            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(_rabbitMqSettings.RetryDelayInSeconds));

                // Recriar mensagem para nova tentativa
                var retryMessage = new EmailQueueMessage
                {
                    EmailLogId = emailLog.Id,
                    RecipientId = emailLog.RecipientId,
                    EmailTemplateId = emailLog.EmailTemplateId,
                    RecipientEmail = emailLog.Recipient.Email,
                    RecipientName = emailLog.Recipient.Name,
                    Subject = emailLog.Subject,
                    Body = emailLog.Body,
                    AttemptCount = emailLog.AttemptCount
                };

                await _messageQueueService.PublishEmailAsync(retryMessage);
                _logger.LogInformation("E-mail reagendado para nova tentativa: {EmailLogId}", emailLog.Id);
            });
        }
        else
        {
            // Marcar como falha definitiva
            emailLog.MarkAsDefinitiveFailure($"Falha definitiva após {emailLog.AttemptCount} tentativas: {errorMessage}");
            await repository.UpdateAsync(emailLog);

            _logger.LogError("E-mail marcado como falha definitiva: {EmailLogId}", emailLog.Id);
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker de processamento de e-mails parando em: {time}", DateTimeOffset.Now);
        await base.StopAsync(stoppingToken);
    }
}
