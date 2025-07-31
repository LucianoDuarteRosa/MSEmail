namespace MSEmail.Application.DTOs;

/// <summary>
/// DTO para requisição de envio de e-mails
/// </summary>
public class SendEmailRequestDto
{
    public Guid EmailTemplateId { get; set; }
    public List<Guid> RecipientIds { get; set; } = new();
    public string? PdfFileName { get; set; }
    public Dictionary<string, string> AdditionalVariables { get; set; } = new();
}

/// <summary>
/// DTO para resposta de envio de e-mails
/// </summary>
public class SendEmailResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<Guid> ProcessedEmailLogIds { get; set; } = new();
}
