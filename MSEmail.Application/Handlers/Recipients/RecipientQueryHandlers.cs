using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using MSEmail.Application.DTOs;
using MSEmail.Application.Queries.Recipients;
using MSEmail.Domain.Interfaces;

namespace MSEmail.Application.Handlers.Recipients;

/// <summary>
/// Handler para obter destinatário por ID
/// </summary>
public class GetRecipientByIdHandler : IRequestHandler<GetRecipientByIdQuery, RecipientDto?>
{
    private readonly IRecipientRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetRecipientByIdHandler> _logger;

    public GetRecipientByIdHandler(
        IRecipientRepository repository,
        IMapper mapper,
        ILogger<GetRecipientByIdHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<RecipientDto?> Handle(GetRecipientByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Obtendo destinatário por ID: {Id}", request.Id);

        var recipient = await _repository.GetByIdAsync(request.Id, cancellationToken);

        return recipient != null ? _mapper.Map<RecipientDto>(recipient) : null;
    }
}

/// <summary>
/// Handler para obter destinatário por e-mail
/// </summary>
public class GetRecipientByEmailHandler : IRequestHandler<GetRecipientByEmailQuery, RecipientDto?>
{
    private readonly IRecipientRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetRecipientByEmailHandler> _logger;

    public GetRecipientByEmailHandler(
        IRecipientRepository repository,
        IMapper mapper,
        ILogger<GetRecipientByEmailHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<RecipientDto?> Handle(GetRecipientByEmailQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Obtendo destinatário por e-mail: {Email}", request.Email);

        var recipient = await _repository.GetByEmailAsync(request.Email, cancellationToken);

        return recipient != null ? _mapper.Map<RecipientDto>(recipient) : null;
    }
}

/// <summary>
/// Handler para obter todos os destinatários
/// </summary>
public class GetAllRecipientsHandler : IRequestHandler<GetAllRecipientsQuery, IEnumerable<RecipientDto>>
{
    private readonly IRecipientRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllRecipientsHandler> _logger;

    public GetAllRecipientsHandler(
        IRecipientRepository repository,
        IMapper mapper,
        ILogger<GetAllRecipientsHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<RecipientDto>> Handle(GetAllRecipientsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Obtendo todos os destinatários. OnlyActive: {OnlyActive}", request.OnlyActive);

        var recipients = request.OnlyActive
            ? await _repository.GetActiveRecipientsAsync(cancellationToken)
            : await _repository.GetAllAsync(cancellationToken);

        return _mapper.Map<IEnumerable<RecipientDto>>(recipients);
    }
}
