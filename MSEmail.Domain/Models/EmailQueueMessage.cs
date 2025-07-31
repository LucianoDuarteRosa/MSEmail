namespace MSEmail.Domain.Models;

/// <summary>
/// Modelo para mensagem de e-mail na fila
/// </summary>
public class EmailQueueMessage
{
    public Guid EmailLogId { get; set; }
    public Guid RecipientId { get; set; }
    public Guid EmailTemplateId { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? PdfPath { get; set; }
    public Dictionary<string, string> Variables { get; set; } = new();
    public int AttemptCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
