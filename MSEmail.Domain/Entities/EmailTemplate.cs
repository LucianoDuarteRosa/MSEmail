using System.ComponentModel.DataAnnotations;

namespace MSEmail.Domain.Entities;

/// <summary>
/// Entidade que representa um template de e-mail configurável
/// </summary>
public class EmailTemplate
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Substitui as variáveis no template pelos valores fornecidos
    /// </summary>
    /// <param name="variables">Dicionário com as variáveis e seus valores</param>
    /// <returns>Template com variáveis substituídas</returns>
    public (string subject, string body) ProcessTemplate(Dictionary<string, string> variables)
    {
        var processedSubject = Subject;
        var processedBody = Body;

        foreach (var variable in variables)
        {
            var placeholder = $"{{{variable.Key}}}";
            processedSubject = processedSubject.Replace(placeholder, variable.Value);
            processedBody = processedBody.Replace(placeholder, variable.Value);
        }

        return (processedSubject, processedBody);
    }
}
