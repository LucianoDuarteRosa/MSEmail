using Microsoft.EntityFrameworkCore;
using MSEmail.Domain.Entities;
using MSEmail.Domain.Interfaces;
using MSEmail.Infrastructure.Data;

namespace MSEmail.Infrastructure.Repositories;

/// <summary>
/// Repositório para destinatários
/// </summary>
public class RecipientRepository : IRecipientRepository
{
    private readonly MSEmailDbContext _context;

    public RecipientRepository(MSEmailDbContext context)
    {
        _context = context;
    }

    public async Task<Recipient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Recipients
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Recipient?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Recipients
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public async Task<IEnumerable<Recipient>> GetActiveRecipientsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Recipients
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Recipient>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Recipients
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Recipient> CreateAsync(Recipient recipient, CancellationToken cancellationToken = default)
    {
        recipient.Id = Guid.NewGuid();
        recipient.CreatedAt = DateTime.UtcNow;
        recipient.UpdatedAt = DateTime.UtcNow;

        _context.Recipients.Add(recipient);
        await _context.SaveChangesAsync(cancellationToken);

        return recipient;
    }

    public async Task<Recipient> UpdateAsync(Recipient recipient, CancellationToken cancellationToken = default)
    {
        recipient.UpdatedAt = DateTime.UtcNow;

        _context.Recipients.Update(recipient);
        await _context.SaveChangesAsync(cancellationToken);

        return recipient;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var recipient = await GetByIdAsync(id, cancellationToken);
        if (recipient != null)
        {
            _context.Recipients.Remove(recipient);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
