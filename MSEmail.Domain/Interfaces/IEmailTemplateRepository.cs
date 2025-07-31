using MSEmail.Domain.Entities;

namespace MSEmail.Domain.Interfaces;

/// <summary>
/// Interface para repositório de templates de e-mail
/// </summary>
public interface IEmailTemplateRepository
{
    Task<EmailTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EmailTemplate?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<EmailTemplate>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<EmailTemplate>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<EmailTemplate> CreateAsync(EmailTemplate template, CancellationToken cancellationToken = default);
    Task<EmailTemplate> UpdateAsync(EmailTemplate template, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
