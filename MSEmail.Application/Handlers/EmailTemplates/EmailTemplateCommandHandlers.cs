using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using MSEmail.Application.Commands.EmailTemplates;
using MSEmail.Application.DTOs;
using MSEmail.Domain.Entities;
using MSEmail.Domain.Interfaces;

namespace MSEmail.Application.Handlers.EmailTemplates;

/// <summary>
/// Handler para criar template de e-mail
/// </summary>
public class CreateEmailTemplateHandler : IRequestHandler<CreateEmailTemplateCommand, EmailTemplateDto>
{
    private readonly IEmailTemplateRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateEmailTemplateHandler> _logger;

    public CreateEmailTemplateHandler(
        IEmailTemplateRepository repository,
        IMapper mapper,
        ILogger<CreateEmailTemplateHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<EmailTemplateDto> Handle(CreateEmailTemplateCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Criando template de e-mail com nome: {Name}", request.EmailTemplate.Name);

        // Verificar se já existe um template com o mesmo nome
        var existingTemplate = await _repository.GetByNameAsync(request.EmailTemplate.Name, cancellationToken);
        if (existingTemplate != null)
        {
            throw new InvalidOperationException($"Template com nome '{request.EmailTemplate.Name}' já existe.");
        }

        var emailTemplate = _mapper.Map<EmailTemplate>(request.EmailTemplate);
        var createdTemplate = await _repository.CreateAsync(emailTemplate, cancellationToken);

        _logger.LogInformation("Template de e-mail criado com sucesso. ID: {Id}", createdTemplate.Id);

        return _mapper.Map<EmailTemplateDto>(createdTemplate);
    }
}

/// <summary>
/// Handler para atualizar template de e-mail
/// </summary>
public class UpdateEmailTemplateHandler : IRequestHandler<UpdateEmailTemplateCommand, EmailTemplateDto>
{
    private readonly IEmailTemplateRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateEmailTemplateHandler> _logger;

    public UpdateEmailTemplateHandler(
        IEmailTemplateRepository repository,
        IMapper mapper,
        ILogger<UpdateEmailTemplateHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<EmailTemplateDto> Handle(UpdateEmailTemplateCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Atualizando template de e-mail com ID: {Id}", request.EmailTemplate.Id);

        var existingTemplate = await _repository.GetByIdAsync(request.EmailTemplate.Id, cancellationToken);
        if (existingTemplate == null)
        {
            throw new ArgumentException($"Template com ID '{request.EmailTemplate.Id}' não encontrado.");
        }

        // Verificar se o novo nome já existe em outro template
        if (existingTemplate.Name != request.EmailTemplate.Name)
        {
            var templateWithSameName = await _repository.GetByNameAsync(request.EmailTemplate.Name, cancellationToken);
            if (templateWithSameName != null && templateWithSameName.Id != request.EmailTemplate.Id)
            {
                throw new InvalidOperationException($"Template com nome '{request.EmailTemplate.Name}' já existe.");
            }
        }

        _mapper.Map(request.EmailTemplate, existingTemplate);
        var updatedTemplate = await _repository.UpdateAsync(existingTemplate, cancellationToken);

        _logger.LogInformation("Template de e-mail atualizado com sucesso. ID: {Id}", updatedTemplate.Id);

        return _mapper.Map<EmailTemplateDto>(updatedTemplate);
    }
}

/// <summary>
/// Handler para deletar template de e-mail
/// </summary>
public class DeleteEmailTemplateHandler : IRequestHandler<DeleteEmailTemplateCommand, bool>
{
    private readonly IEmailTemplateRepository _repository;
    private readonly ILogger<DeleteEmailTemplateHandler> _logger;

    public DeleteEmailTemplateHandler(
        IEmailTemplateRepository repository,
        ILogger<DeleteEmailTemplateHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteEmailTemplateCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deletando template de e-mail com ID: {Id}", request.Id);

        var existingTemplate = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (existingTemplate == null)
        {
            _logger.LogWarning("Template com ID '{Id}' não encontrado para exclusão", request.Id);
            return false;
        }

        await _repository.DeleteAsync(request.Id, cancellationToken);

        _logger.LogInformation("Template de e-mail deletado com sucesso. ID: {Id}", request.Id);

        return true;
    }
}
