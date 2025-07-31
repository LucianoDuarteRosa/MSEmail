using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using MSEmail.Application.DTOs;
using MSEmail.Application.Queries.Emails;
using MSEmail.Domain.Entities;
using MSEmail.Domain.Interfaces;

namespace MSEmail.Application.Handlers.Emails;

/// <summary>
/// Handler para obter logs de e-mail por status
/// </summary>
public class GetEmailLogsByStatusHandler : IRequestHandler<GetEmailLogsByStatusQuery, IEnumerable<EmailLogDto>>
{
    private readonly IEmailLogRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetEmailLogsByStatusHandler> _logger;

    public GetEmailLogsByStatusHandler(
        IEmailLogRepository repository,
        IMapper mapper,
        ILogger<GetEmailLogsByStatusHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<EmailLogDto>> Handle(GetEmailLogsByStatusQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando logs de e-mail por status: {Status}", request.Status);

        var emailLogs = await _repository.GetByStatusAsync(request.Status, cancellationToken);
        return _mapper.Map<IEnumerable<EmailLogDto>>(emailLogs);
    }
}

/// <summary>
/// Handler para obter log de e-mail por ID
/// </summary>
public class GetEmailLogByIdHandler : IRequestHandler<GetEmailLogByIdQuery, EmailLogDto?>
{
    private readonly IEmailLogRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetEmailLogByIdHandler> _logger;

    public GetEmailLogByIdHandler(
        IEmailLogRepository repository,
        IMapper mapper,
        ILogger<GetEmailLogByIdHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<EmailLogDto?> Handle(GetEmailLogByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando log de e-mail por ID: {Id}", request.Id);

        var emailLog = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (emailLog == null)
        {
            _logger.LogWarning("Log de e-mail com ID '{Id}' não encontrado", request.Id);
            return null;
        }

        return _mapper.Map<EmailLogDto>(emailLog);
    }
}

/// <summary>
/// Handler para obter estatísticas de e-mails
/// </summary>
public class GetEmailStatisticsHandler : IRequestHandler<GetEmailStatisticsQuery, EmailStatisticsDto>
{
    private readonly IEmailLogRepository _repository;
    private readonly ILogger<GetEmailStatisticsHandler> _logger;

    public GetEmailStatisticsHandler(
        IEmailLogRepository repository,
        ILogger<GetEmailStatisticsHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<EmailStatisticsDto> Handle(GetEmailStatisticsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Gerando estatísticas de e-mails");

        var pendingEmails = await _repository.GetByStatusAsync(EmailStatus.Pending, cancellationToken);
        var processingEmails = await _repository.GetByStatusAsync(EmailStatus.Processing, cancellationToken);
        var sentEmails = await _repository.GetByStatusAsync(EmailStatus.Sent, cancellationToken);
        var failedEmails = await _repository.GetByStatusAsync(EmailStatus.Failed, cancellationToken);
        var retryingEmails = await _repository.GetByStatusAsync(EmailStatus.Retrying, cancellationToken);

        var pendingCount = pendingEmails.Count();
        var processingCount = processingEmails.Count();
        var sentCount = sentEmails.Count();
        var failedCount = failedEmails.Count();
        var retryingCount = retryingEmails.Count();

        return new EmailStatisticsDto
        {
            PendingEmails = pendingCount,
            ProcessingEmails = processingCount,
            SentEmails = sentCount,
            FailedEmails = failedCount,
            RetryingEmails = retryingCount,
            TotalEmails = pendingCount + processingCount + sentCount + failedCount + retryingCount
        };
    }
}

/// <summary>
/// Handler para obter logs de e-mail por destinatário
/// </summary>
public class GetEmailLogsByRecipientHandler : IRequestHandler<GetEmailLogsByRecipientQuery, IEnumerable<EmailLogDto>>
{
    private readonly IEmailLogRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetEmailLogsByRecipientHandler> _logger;

    public GetEmailLogsByRecipientHandler(
        IEmailLogRepository repository,
        IMapper mapper,
        ILogger<GetEmailLogsByRecipientHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<EmailLogDto>> Handle(GetEmailLogsByRecipientQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando logs de e-mail por destinatário: {RecipientId}", request.RecipientId);

        var emailLogs = await _repository.GetByRecipientIdAsync(request.RecipientId, cancellationToken);
        return _mapper.Map<IEnumerable<EmailLogDto>>(emailLogs);
    }
}
