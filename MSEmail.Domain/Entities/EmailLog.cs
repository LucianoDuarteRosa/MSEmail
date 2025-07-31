using System.ComponentModel.DataAnnotations;

namespace MSEmail.Domain.Entities;

/// <summary>
/// Status de envio de um e-mail
/// </summary>
public enum EmailStatus
{
    Pending = 0,
    Processing = 1,
    Sent = 2,
    Failed = 3,
    Retrying = 4
}

/// <summary>
/// Entidade que representa o log de envio de e-mails
/// </summary>
public class EmailLog
{
    [Key]
    public Guid Id { get; set; }

    public Guid RecipientId { get; set; }
    public virtual Recipient Recipient { get; set; } = null!;

    public Guid EmailTemplateId { get; set; }
    public virtual EmailTemplate EmailTemplate { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    public EmailStatus Status { get; set; } = EmailStatus.Pending;

    public int AttemptCount { get; set; } = 0;

    public int MaxAttempts { get; set; } = 3;

    public string? ErrorMessage { get; set; }

    public DateTime? SentAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Verifica se deve tentar reenviar o e-mail
    /// </summary>
    /// <returns>True se deve tentar reenviar</returns>
    public bool ShouldRetry()
    {
        return Status == EmailStatus.Failed && AttemptCount < MaxAttempts;
    }

    /// <summary>
    /// Marca como falha definitiva se excedeu o número máximo de tentativas
    /// </summary>
    public void MarkAsDefinitiveFailure(string errorMessage)
    {
        if (AttemptCount >= MaxAttempts)
        {
            Status = EmailStatus.Failed;
            ErrorMessage = errorMessage;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Incrementa o contador de tentativas
    /// </summary>
    public void IncrementAttempt()
    {
        AttemptCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marca como enviado com sucesso
    /// </summary>
    public void MarkAsSent()
    {
        Status = EmailStatus.Sent;
        SentAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
