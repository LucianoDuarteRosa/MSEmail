using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using MSEmail.Application.Commands.Recipients;
using MSEmail.Application.DTOs;
using MSEmail.Domain.Entities;
using MSEmail.Domain.Interfaces;

namespace MSEmail.Application.Handlers.Recipients;

/// <summary>
/// Handler para criar destinatário
/// </summary>
public class CreateRecipientHandler : IRequestHandler<CreateRecipientCommand, RecipientDto>
{
    private readonly IRecipientRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateRecipientHandler> _logger;

    public CreateRecipientHandler(
        IRecipientRepository repository,
        IMapper mapper,
        ILogger<CreateRecipientHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<RecipientDto> Handle(CreateRecipientCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Criando destinatário com e-mail: {Email}", request.Recipient.Email);

        // Verificar se já existe um destinatário com o mesmo e-mail
        var existingRecipient = await _repository.GetByEmailAsync(request.Recipient.Email, cancellationToken);
        if (existingRecipient != null)
        {
            throw new InvalidOperationException($"Destinatário com e-mail '{request.Recipient.Email}' já existe.");
        }

        var recipient = _mapper.Map<Recipient>(request.Recipient);
        var createdRecipient = await _repository.CreateAsync(recipient, cancellationToken);

        _logger.LogInformation("Destinatário criado com sucesso. ID: {Id}", createdRecipient.Id);

        return _mapper.Map<RecipientDto>(createdRecipient);
    }
}

/// <summary>
/// Handler para atualizar destinatário
/// </summary>
public class UpdateRecipientHandler : IRequestHandler<UpdateRecipientCommand, RecipientDto>
{
    private readonly IRecipientRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateRecipientHandler> _logger;

    public UpdateRecipientHandler(
        IRecipientRepository repository,
        IMapper mapper,
        ILogger<UpdateRecipientHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<RecipientDto> Handle(UpdateRecipientCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Atualizando destinatário com ID: {Id}", request.Recipient.Id);

        var existingRecipient = await _repository.GetByIdAsync(request.Recipient.Id, cancellationToken);
        if (existingRecipient == null)
        {
            throw new ArgumentException($"Destinatário com ID '{request.Recipient.Id}' não encontrado.");
        }

        // Verificar se o e-mail não está sendo usado por outro destinatário
        var recipientWithSameEmail = await _repository.GetByEmailAsync(request.Recipient.Email, cancellationToken);
        if (recipientWithSameEmail != null && recipientWithSameEmail.Id != request.Recipient.Id)
        {
            throw new InvalidOperationException($"E-mail '{request.Recipient.Email}' já está sendo usado por outro destinatário.");
        }

        _mapper.Map(request.Recipient, existingRecipient);
        var updatedRecipient = await _repository.UpdateAsync(existingRecipient, cancellationToken);

        _logger.LogInformation("Destinatário atualizado com sucesso. ID: {Id}", updatedRecipient.Id);

        return _mapper.Map<RecipientDto>(updatedRecipient);
    }
}

/// <summary>
/// Handler para deletar destinatário
/// </summary>
public class DeleteRecipientHandler : IRequestHandler<DeleteRecipientCommand, bool>
{
    private readonly IRecipientRepository _repository;
    private readonly ILogger<DeleteRecipientHandler> _logger;

    public DeleteRecipientHandler(
        IRecipientRepository repository,
        ILogger<DeleteRecipientHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteRecipientCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deletando destinatário com ID: {Id}", request.Id);

        var existingRecipient = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (existingRecipient == null)
        {
            _logger.LogWarning("Destinatário com ID {Id} não encontrado para exclusão", request.Id);
            return false;
        }

        await _repository.DeleteAsync(request.Id, cancellationToken);

        _logger.LogInformation("Destinatário deletado com sucesso. ID: {Id}", request.Id);

        return true;
    }
}
