using MediatR;
using MSEmail.Application.DTOs;

namespace MSEmail.Application.Queries.Recipients;

/// <summary>
/// Query para obter destinatário por ID
/// </summary>
public record GetRecipientByIdQuery(Guid Id) : IRequest<RecipientDto?>;

/// <summary>
/// Query para obter destinatário por e-mail
/// </summary>
public record GetRecipientByEmailQuery(string Email) : IRequest<RecipientDto?>;

/// <summary>
/// Query para obter todos os destinatários
/// </summary>
public record GetAllRecipientsQuery(bool OnlyActive = false) : IRequest<IEnumerable<RecipientDto>>;
