using MediatR;
using MSEmail.Application.DTOs;

namespace MSEmail.Application.Commands.Recipients;

/// <summary>
/// Comando para criar destinatário
/// </summary>
public record CreateRecipientCommand(CreateRecipientDto Recipient) : IRequest<RecipientDto>;

/// <summary>
/// Comando para atualizar destinatário
/// </summary>
public record UpdateRecipientCommand(UpdateRecipientDto Recipient) : IRequest<RecipientDto>;

/// <summary>
/// Comando para deletar destinatário
/// </summary>
public record DeleteRecipientCommand(Guid Id) : IRequest<bool>;
