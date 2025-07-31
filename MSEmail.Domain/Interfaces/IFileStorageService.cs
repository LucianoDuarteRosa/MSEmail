namespace MSEmail.Domain.Interfaces;

/// <summary>
/// Interface para serviço de armazenamento de arquivos
/// </summary>
public interface IFileStorageService
{
    Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default);
    Task<byte[]> GetFileAsync(string filePath, CancellationToken cancellationToken = default);
    Task<string> SaveFileAsync(byte[] fileContent, string fileName, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
}
