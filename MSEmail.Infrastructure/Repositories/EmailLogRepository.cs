using Microsoft.EntityFrameworkCore;
using MSEmail.Domain.Entities;
using MSEmail.Domain.Interfaces;
using MSEmail.Infrastructure.Data;

namespace MSEmail.Infrastructure.Repositories;

/// <summary>
/// Repositório para logs de e-mail
/// </summary>
public class EmailLogRepository : IEmailLogRepository
{
    private readonly MSEmailDbContext _context;

    public EmailLogRepository(MSEmailDbContext context)
    {
        _context = context;
    }

    public async Task<EmailLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.EmailLogs
            .Include(x => x.Recipient)
            .Include(x => x.EmailTemplate)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<EmailLog>> GetByStatusAsync(EmailStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.EmailLogs
            .Include(x => x.Recipient)
            .Include(x => x.EmailTemplate)
            .Where(x => x.Status == status)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<EmailLog>> GetPendingEmailsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.EmailLogs
            .Include(x => x.Recipient)
            .Include(x => x.EmailTemplate)
            .Where(x => x.Status == EmailStatus.Pending)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<EmailLog>> GetFailedEmailsForRetryAsync(CancellationToken cancellationToken = default)
    {
        return await _context.EmailLogs
            .Include(x => x.Recipient)
            .Include(x => x.EmailTemplate)
            .Where(x => x.Status == EmailStatus.Failed && x.AttemptCount < x.MaxAttempts)
            .OrderBy(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<EmailLog>> GetByRecipientIdAsync(Guid recipientId, CancellationToken cancellationToken = default)
    {
        return await _context.EmailLogs
            .Include(x => x.Recipient)
            .Include(x => x.EmailTemplate)
            .Where(x => x.RecipientId == recipientId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<EmailLog> CreateAsync(EmailLog emailLog, CancellationToken cancellationToken = default)
    {
        emailLog.Id = Guid.NewGuid();
        emailLog.CreatedAt = DateTime.UtcNow;
        emailLog.UpdatedAt = DateTime.UtcNow;

        _context.EmailLogs.Add(emailLog);
        await _context.SaveChangesAsync(cancellationToken);

        return emailLog;
    }

    public async Task<EmailLog> UpdateAsync(EmailLog emailLog, CancellationToken cancellationToken = default)
    {
        emailLog.UpdatedAt = DateTime.UtcNow;

        _context.EmailLogs.Update(emailLog);
        await _context.SaveChangesAsync(cancellationToken);

        return emailLog;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var emailLog = await _context.EmailLogs.FindAsync(new object[] { id }, cancellationToken);
        if (emailLog != null)
        {
            _context.EmailLogs.Remove(emailLog);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
