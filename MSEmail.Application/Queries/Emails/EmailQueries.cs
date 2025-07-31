using MediatR;
using MSEmail.Application.DTOs;
using MSEmail.Domain.Entities;

namespace MSEmail.Application.Queries.Emails;

/// <summary>
/// Query para obter logs de e-mail por status
/// </summary>
public record GetEmailLogsByStatusQuery(EmailStatus Status) : IRequest<IEnumerable<EmailLogDto>>;

/// <summary>
/// Query para obter log de e-mail por ID
/// </summary>
public record GetEmailLogByIdQuery(Guid Id) : IRequest<EmailLogDto?>;

/// <summary>
/// Query para obter estatísticas de e-mails
/// </summary>
public record GetEmailStatisticsQuery : IRequest<EmailStatisticsDto>;

/// <summary>
/// Query para obter logs de e-mail por destinatário
/// </summary>
public record GetEmailLogsByRecipientQuery(Guid RecipientId) : IRequest<IEnumerable<EmailLogDto>>;
