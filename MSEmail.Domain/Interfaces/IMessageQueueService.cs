namespace MSEmail.Domain.Interfaces;

/// <summary>
/// Interface para serviço de fila de mensagens
/// </summary>
public interface IMessageQueueService
{
    Task PublishEmailAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;
    Task<bool> ConsumeEmailAsync<T>(Func<T, Task> onMessageReceived, CancellationToken cancellationToken = default) where T : class;
}
