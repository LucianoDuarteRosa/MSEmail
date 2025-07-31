using MSEmail.Domain.Entities;

namespace MSEmail.Domain.Interfaces;

/// <summary>
/// Interface para repositório de logs de e-mail
/// </summary>
public interface IEmailLogRepository
{
    Task<EmailLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<EmailLog>> GetByStatusAsync(EmailStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<EmailLog>> GetPendingEmailsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<EmailLog>> GetFailedEmailsForRetryAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<EmailLog>> GetByRecipientIdAsync(Guid recipientId, CancellationToken cancellationToken = default);
    Task<EmailLog> CreateAsync(EmailLog emailLog, CancellationToken cancellationToken = default);
    Task<EmailLog> UpdateAsync(EmailLog emailLog, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
