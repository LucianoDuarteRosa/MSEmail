using Microsoft.EntityFrameworkCore;
using MSEmail.Domain.Entities;
using MSEmail.Domain.Interfaces;
using MSEmail.Infrastructure.Data;

namespace MSEmail.Infrastructure.Repositories;

/// <summary>
/// Repositório para templates de e-mail
/// </summary>
public class EmailTemplateRepository : IEmailTemplateRepository
{
    private readonly MSEmailDbContext _context;

    public EmailTemplateRepository(MSEmailDbContext context)
    {
        _context = context;
    }

    public async Task<EmailTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.EmailTemplates
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<EmailTemplate?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.EmailTemplates
            .FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<EmailTemplate>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.EmailTemplates
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<EmailTemplate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.EmailTemplates
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<EmailTemplate> CreateAsync(EmailTemplate template, CancellationToken cancellationToken = default)
    {
        template.Id = Guid.NewGuid();
        template.CreatedAt = DateTime.UtcNow;
        template.UpdatedAt = DateTime.UtcNow;

        _context.EmailTemplates.Add(template);
        await _context.SaveChangesAsync(cancellationToken);

        return template;
    }

    public async Task<EmailTemplate> UpdateAsync(EmailTemplate template, CancellationToken cancellationToken = default)
    {
        template.UpdatedAt = DateTime.UtcNow;

        _context.EmailTemplates.Update(template);
        await _context.SaveChangesAsync(cancellationToken);

        return template;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var template = await GetByIdAsync(id, cancellationToken);
        if (template != null)
        {
            _context.EmailTemplates.Remove(template);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
