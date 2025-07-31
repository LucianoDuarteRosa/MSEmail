using MSEmail.Domain.Entities;

namespace MSEmail.Domain.Interfaces;

/// <summary>
/// Interface para repositório de destinatários
/// </summary>
public interface IRecipientRepository
{
    Task<Recipient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Recipient?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<Recipient>> GetActiveRecipientsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Recipient>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Recipient> CreateAsync(Recipient recipient, CancellationToken cancellationToken = default);
    Task<Recipient> UpdateAsync(Recipient recipient, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
