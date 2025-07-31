namespace MSEmail.Domain.Models;

/// <summary>
/// Configurações para armazenamento de arquivos
/// </summary>
public class FileStorageSettings
{
    public const string SectionName = "FileStorageSettings";

    public string Type { get; set; } = "Local"; // Local, AzureBlob, etc.
    public string BasePath { get; set; } = "C:\\Files\\Invoices";
    public string? ConnectionString { get; set; }
    public string? ContainerName { get; set; }
}
