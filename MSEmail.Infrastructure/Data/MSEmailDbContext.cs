using Microsoft.EntityFrameworkCore;
using MSEmail.Domain.Entities;

namespace MSEmail.Infrastructure.Data;

/// <summary>
/// Contexto do banco de dados para o sistema de e-mail
/// </summary>
public class MSEmailDbContext : DbContext
{
    public MSEmailDbContext(DbContextOptions<MSEmailDbContext> options) : base(options)
    {
    }

    public DbSet<EmailTemplate> EmailTemplates { get; set; }
    public DbSet<Recipient> Recipients { get; set; }
    public DbSet<EmailLog> EmailLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração de EmailTemplate
        modelBuilder.Entity<EmailTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Body).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Configuração de Recipient
        modelBuilder.Entity<Recipient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Cdc).IsRequired().HasMaxLength(20);
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Configuração de EmailLog
        modelBuilder.Entity<EmailLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Body).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.AttemptCount).IsRequired();
            entity.Property(e => e.MaxAttempts).IsRequired();
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            // Relacionamentos
            entity.HasOne(e => e.Recipient)
                .WithMany()
                .HasForeignKey(e => e.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.EmailTemplate)
                .WithMany()
                .HasForeignKey(e => e.EmailTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices para performance
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.RecipientId);
        });
    }
}
