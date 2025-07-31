using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using MSEmail.Application.DTOs;
using MSEmail.Application.Queries.EmailTemplates;
using MSEmail.Domain.Interfaces;

namespace MSEmail.Application.Handlers.EmailTemplates;

/// <summary>
/// Handler para obter template de e-mail por ID
/// </summary>
public class GetEmailTemplateByIdHandler : IRequestHandler<GetEmailTemplateByIdQuery, EmailTemplateDto?>
{
    private readonly IEmailTemplateRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetEmailTemplateByIdHandler> _logger;

    public GetEmailTemplateByIdHandler(
        IEmailTemplateRepository repository,
        IMapper mapper,
        ILogger<GetEmailTemplateByIdHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<EmailTemplateDto?> Handle(GetEmailTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando template de e-mail por ID: {Id}", request.Id);

        var emailTemplate = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (emailTemplate == null)
        {
            _logger.LogWarning("Template de e-mail com ID '{Id}' não encontrado", request.Id);
            return null;
        }

        return _mapper.Map<EmailTemplateDto>(emailTemplate);
    }
}

/// <summary>
/// Handler para obter template de e-mail por nome
/// </summary>
public class GetEmailTemplateByNameHandler : IRequestHandler<GetEmailTemplateByNameQuery, EmailTemplateDto?>
{
    private readonly IEmailTemplateRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetEmailTemplateByNameHandler> _logger;

    public GetEmailTemplateByNameHandler(
        IEmailTemplateRepository repository,
        IMapper mapper,
        ILogger<GetEmailTemplateByNameHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<EmailTemplateDto?> Handle(GetEmailTemplateByNameQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando template de e-mail por nome: {Name}", request.Name);

        var emailTemplate = await _repository.GetByNameAsync(request.Name, cancellationToken);
        if (emailTemplate == null)
        {
            _logger.LogWarning("Template de e-mail com nome '{Name}' não encontrado", request.Name);
            return null;
        }

        return _mapper.Map<EmailTemplateDto>(emailTemplate);
    }
}

/// <summary>
/// Handler para obter todos os templates de e-mail
/// </summary>
public class GetAllEmailTemplatesHandler : IRequestHandler<GetAllEmailTemplatesQuery, IEnumerable<EmailTemplateDto>>
{
    private readonly IEmailTemplateRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllEmailTemplatesHandler> _logger;

    public GetAllEmailTemplatesHandler(
        IEmailTemplateRepository repository,
        IMapper mapper,
        ILogger<GetAllEmailTemplatesHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<EmailTemplateDto>> Handle(GetAllEmailTemplatesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando todos os templates de e-mail. OnlyActive: {OnlyActive}", request.OnlyActive);

        var emailTemplates = request.OnlyActive
            ? await _repository.GetAllActiveAsync(cancellationToken)
            : await _repository.GetAllAsync(cancellationToken);

        return _mapper.Map<IEnumerable<EmailTemplateDto>>(emailTemplates);
    }
}
