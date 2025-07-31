using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using MSEmail.Application.Commands.Emails;
using MSEmail.Application.DTOs;
using MSEmail.Domain.Entities;
using MSEmail.Domain.Interfaces;
using MSEmail.Domain.Models;

namespace MSEmail.Application.Handlers.Emails;

/// <summary>
/// Handler para enviar e-mails
/// </summary>
public class SendEmailHandler : IRequestHandler<SendEmailCommand, SendEmailResponseDto>
{
    private readonly IEmailTemplateRepository _emailTemplateRepository;
    private readonly IRecipientRepository _recipientRepository;
    private readonly IEmailLogRepository _emailLogRepository;
    private readonly IMessageQueueService _messageQueueService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<SendEmailHandler> _logger;

    public SendEmailHandler(
        IEmailTemplateRepository emailTemplateRepository,
        IRecipientRepository recipientRepository,
        IEmailLogRepository emailLogRepository,
        IMessageQueueService messageQueueService,
        IFileStorageService fileStorageService,
        ILogger<SendEmailHandler> logger)
    {
        _emailTemplateRepository = emailTemplateRepository;
        _recipientRepository = recipientRepository;
        _emailLogRepository = emailLogRepository;
        _messageQueueService = messageQueueService;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<SendEmailResponseDto> Handle(SendEmailCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando processo de envio de e-mails para {Count} destinatários", request.Request.RecipientIds.Count);

        try
        {
            // Buscar template
            var template = await _emailTemplateRepository.GetByIdAsync(request.Request.EmailTemplateId, cancellationToken);
            if (template == null)
            {
                return new SendEmailResponseDto
                {
                    Success = false,
                    Message = "Template de e-mail não encontrado."
                };
            }

            // Verificar se arquivo PDF existe, se especificado
            string? pdfPath = null;
            if (!string.IsNullOrEmpty(request.Request.PdfFileName))
            {
                if (!await _fileStorageService.FileExistsAsync(request.Request.PdfFileName, cancellationToken))
                {
                    return new SendEmailResponseDto
                    {
                        Success = false,
                        Message = $"Arquivo PDF '{request.Request.PdfFileName}' não encontrado."
                    };
                }
                pdfPath = request.Request.PdfFileName;
            }

            var processedEmailLogIds = new List<Guid>();

            // Processar cada destinatário
            foreach (var recipientId in request.Request.RecipientIds)
            {
                var recipient = await _recipientRepository.GetByIdAsync(recipientId, cancellationToken);
                if (recipient == null)
                {
                    _logger.LogWarning("Destinatário com ID '{RecipientId}' não encontrado", recipientId);
                    continue;
                }

                // Preparar variáveis para substituição
                var variables = recipient.GetTemplateVariables();
                foreach (var additionalVar in request.Request.AdditionalVariables)
                {
                    variables[additionalVar.Key] = additionalVar.Value;
                }

                // Processar template
                var (processedSubject, processedBody) = template.ProcessTemplate(variables);

                // Criar log de e-mail
                var emailLog = new EmailLog
                {
                    RecipientId = recipientId,
                    EmailTemplateId = template.Id,
                    Subject = processedSubject,
                    Body = processedBody,
                    Status = EmailStatus.Pending
                };

                var createdEmailLog = await _emailLogRepository.CreateAsync(emailLog, cancellationToken);
                processedEmailLogIds.Add(createdEmailLog.Id);

                // Criar mensagem para a fila
                var queueMessage = new EmailQueueMessage
                {
                    EmailLogId = createdEmailLog.Id,
                    RecipientId = recipientId,
                    EmailTemplateId = template.Id,
                    RecipientEmail = recipient.Email,
                    RecipientName = recipient.Name,
                    Subject = processedSubject,
                    Body = processedBody,
                    PdfPath = pdfPath,
                    Variables = variables
                };

                // Enviar para fila
                await _messageQueueService.PublishEmailAsync(queueMessage, cancellationToken);

                _logger.LogInformation("E-mail adicionado à fila para destinatário: {Email}", recipient.Email);
            }

            return new SendEmailResponseDto
            {
                Success = true,
                Message = $"{processedEmailLogIds.Count} e-mails adicionados à fila de processamento.",
                ProcessedEmailLogIds = processedEmailLogIds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar envio de e-mails");
            return new SendEmailResponseDto
            {
                Success = false,
                Message = "Erro interno ao processar e-mails."
            };
        }
    }
}

/// <summary>
/// Handler para reprocessar e-mail específico
/// </summary>
public class ReprocessEmailHandler : IRequestHandler<ReprocessEmailCommand, bool>
{
    private readonly IEmailLogRepository _emailLogRepository;
    private readonly IMessageQueueService _messageQueueService;
    private readonly ILogger<ReprocessEmailHandler> _logger;

    public ReprocessEmailHandler(
        IEmailLogRepository emailLogRepository,
        IMessageQueueService messageQueueService,
        ILogger<ReprocessEmailHandler> logger)
    {
        _emailLogRepository = emailLogRepository;
        _messageQueueService = messageQueueService;
        _logger = logger;
    }

    public async Task<bool> Handle(ReprocessEmailCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Reprocessando e-mail com ID: {EmailLogId}", request.EmailLogId);

        var emailLog = await _emailLogRepository.GetByIdAsync(request.EmailLogId, cancellationToken);
        if (emailLog == null)
        {
            _logger.LogWarning("EmailLog com ID '{EmailLogId}' não encontrado", request.EmailLogId);
            return false;
        }

        // Resetar status para reprocessamento
        emailLog.Status = EmailStatus.Pending;
        emailLog.ErrorMessage = null;
        emailLog.UpdatedAt = DateTime.UtcNow;

        await _emailLogRepository.UpdateAsync(emailLog, cancellationToken);

        // Criar mensagem para reprocessamento
        var queueMessage = new EmailQueueMessage
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

        await _messageQueueService.PublishEmailAsync(queueMessage, cancellationToken);

        _logger.LogInformation("E-mail reprocessado com sucesso. EmailLogId: {EmailLogId}", request.EmailLogId);

        return true;
    }
}
