using MSEmail.Domain.Entities;

namespace MSEmail.Application.DTOs;

/// <summary>
/// DTO para log de e-mail
/// </summary>
public class EmailLogDto
{
    public Guid Id { get; set; }
    public Guid RecipientId { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientEmail { get; set; } = string.Empty;
    public Guid EmailTemplateId { get; set; }
    public string EmailTemplateName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public EmailStatus Status { get; set; }
    public int AttemptCount { get; set; }
    public int MaxAttempts { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO para estatísticas de envio de e-mails
/// </summary>
public class EmailStatisticsDto
{
    public int TotalEmails { get; set; }
    public int PendingEmails { get; set; }
    public int ProcessingEmails { get; set; }
    public int SentEmails { get; set; }
    public int FailedEmails { get; set; }
    public int RetryingEmails { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
