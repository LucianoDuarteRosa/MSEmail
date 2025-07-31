using MediatR;
using MSEmail.Application.DTOs;

namespace MSEmail.Application.Commands.EmailTemplates;

/// <summary>
/// Comando para criar template de e-mail
/// </summary>
public record CreateEmailTemplateCommand(CreateEmailTemplateDto EmailTemplate) : IRequest<EmailTemplateDto>;

/// <summary>
/// Comando para atualizar template de e-mail
/// </summary>
public record UpdateEmailTemplateCommand(UpdateEmailTemplateDto EmailTemplate) : IRequest<EmailTemplateDto>;

/// <summary>
/// Comando para deletar template de e-mail
/// </summary>
public record DeleteEmailTemplateCommand(Guid Id) : IRequest<bool>;
