using System.ComponentModel.DataAnnotations;

namespace MSEmail.Domain.Entities;

/// <summary>
/// Entidade que representa um destinatário de e-mail
/// </summary>
public class Recipient
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Cdc { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gera as variáveis de substituição para este destinatário
    /// </summary>
    /// <returns>Dicionário com as variáveis disponíveis</returns>
    public Dictionary<string, string> GetTemplateVariables()
    {
        return new Dictionary<string, string>
        {
            { "usuario.nome", Name },
            { "usuario.email", Email },
            { "usuario.cdc", Cdc }
        };
    }
}
