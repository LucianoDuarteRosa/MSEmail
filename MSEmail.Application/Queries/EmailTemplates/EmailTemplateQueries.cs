using MediatR;
using MSEmail.Application.DTOs;

namespace MSEmail.Application.Queries.EmailTemplates;

/// <summary>
/// Query para obter template de e-mail por ID
/// </summary>
public record GetEmailTemplateByIdQuery(Guid Id) : IRequest<EmailTemplateDto?>;

/// <summary>
/// Query para obter template de e-mail por nome
/// </summary>
public record GetEmailTemplateByNameQuery(string Name) : IRequest<EmailTemplateDto?>;

/// <summary>
/// Query para obter todos os templates de e-mail
/// </summary>
public record GetAllEmailTemplatesQuery(bool OnlyActive = false) : IRequest<IEnumerable<EmailTemplateDto>>;
