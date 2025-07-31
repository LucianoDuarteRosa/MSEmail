using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MSEmail.Domain.Interfaces;
using MSEmail.Domain.Models;

namespace MSEmail.Infrastructure.Services;

/// <summary>
/// Serviço de armazenamento de arquivos local
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly FileStorageSettings _settings;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(IOptions<FileStorageSettings> settings, ILogger<LocalFileStorageService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        // Criar diretório base se não existir
        if (!Directory.Exists(_settings.BasePath))
        {
            Directory.CreateDirectory(_settings.BasePath);
            _logger.LogInformation("Diretório criado: {BasePath}", _settings.BasePath);
        }
    }

    public Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullPath = GetFullPath(filePath);
            return Task.FromResult(File.Exists(fullPath));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar existência do arquivo: {FilePath}", filePath);
            return Task.FromResult(false);
        }
    }

    public async Task<byte[]> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullPath = GetFullPath(filePath);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"Arquivo não encontrado: {filePath}");
            }

            _logger.LogInformation("Lendo arquivo: {FilePath}", filePath);
            return await File.ReadAllBytesAsync(fullPath, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao ler arquivo: {FilePath}", filePath);
            throw;
        }
    }

    public async Task<string> SaveFileAsync(byte[] fileContent, string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullPath = GetFullPath(fileName);
            var directory = Path.GetDirectoryName(fullPath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllBytesAsync(fullPath, fileContent, cancellationToken);

            _logger.LogInformation("Arquivo salvo: {FilePath}", fileName);
            return fileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar arquivo: {FileName}", fileName);
            throw;
        }
    }

    public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullPath = GetFullPath(filePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("Arquivo deletado: {FilePath}", filePath);
            }
            else
            {
                _logger.LogWarning("Arquivo não encontrado para exclusão: {FilePath}", filePath);
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar arquivo: {FilePath}", filePath);
            throw;
        }
    }

    private string GetFullPath(string relativePath)
    {
        // Garantir que o caminho seja seguro e não saia do diretório base
        var fullPath = Path.Combine(_settings.BasePath, relativePath.TrimStart('/', '\\'));
        var normalizedPath = Path.GetFullPath(fullPath);
        var normalizedBasePath = Path.GetFullPath(_settings.BasePath);

        if (!normalizedPath.StartsWith(normalizedBasePath))
        {
            throw new UnauthorizedAccessException("Caminho de arquivo não autorizado");
        }

        return normalizedPath;
    }
}
