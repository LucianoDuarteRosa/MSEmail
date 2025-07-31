using MediatR;
using MSEmail.Application.DTOs;

namespace MSEmail.Application.Commands.Emails;

/// <summary>
/// Comando para enviar e-mails
/// </summary>
public record SendEmailCommand(SendEmailRequestDto Request) : IRequest<SendEmailResponseDto>;

/// <summary>
/// Comando para reprocessar e-mail específico
/// </summary>
public record ReprocessEmailCommand(Guid EmailLogId) : IRequest<bool>;

/// <summary>
/// Comando para processar e-mails pendentes
/// </summary>
public record ProcessPendingEmailsCommand : IRequest<int>;
