using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MSEmail.Domain.Interfaces;
using MSEmail.Domain.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace MSEmail.Infrastructure.Services;

/// <summary>
/// Serviço de fila de mensagens usando RabbitMQ
/// </summary>
public class RabbitMqService : IMessageQueueService, IDisposable
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqService> _logger;
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    public RabbitMqService(IOptions<RabbitMqSettings> settings, ILogger<RabbitMqService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.Username,
            Password = _settings.Password,
            VirtualHost = _settings.VirtualHost
        };

        _connection = factory.CreateConnectionAsync().Result;
        _channel = _connection.CreateChannelAsync().Result;

        // Declarar exchange e queue
        _channel.ExchangeDeclareAsync(
            exchange: _settings.EmailExchangeName,
            type: ExchangeType.Direct,
            durable: _settings.Durable).Wait();

        _channel.QueueDeclareAsync(
            queue: _settings.EmailQueueName,
            durable: _settings.Durable,
            exclusive: false,
            autoDelete: false).Wait();

        _channel.QueueBindAsync(
            queue: _settings.EmailQueueName,
            exchange: _settings.EmailExchangeName,
            routingKey: _settings.EmailRoutingKey).Wait();

        _logger.LogInformation("RabbitMQ conectado e configurado");
    }

    public async Task PublishEmailAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = new BasicProperties
            {
                Persistent = _settings.Durable,
                MessageId = Guid.NewGuid().ToString(),
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            await _channel.BasicPublishAsync(
                exchange: _settings.EmailExchangeName,
                routingKey: _settings.EmailRoutingKey,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Mensagem publicada na fila: {MessageId}", properties.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar mensagem na fila");
            throw;
        }
    }

    public async Task<bool> ConsumeEmailAsync<T>(Func<T, Task> onMessageReceived, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                try
                {
                    var message = JsonSerializer.Deserialize<T>(json);
                    if (message != null)
                    {
                        await onMessageReceived(message);
                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                        _logger.LogInformation("Mensagem processada com sucesso: {MessageId}", ea.BasicProperties.MessageId);
                    }
                    else
                    {
                        _logger.LogWarning("Não foi possível deserializar a mensagem");
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagem: {MessageId}", ea.BasicProperties.MessageId);

                    // Verificar se deve rejeitar ou tentar novamente
                    var retryCount = GetRetryCount(ea.BasicProperties);
                    if (retryCount < _settings.MaxRetryAttempts)
                    {
                        // Rejeitar e reenviar para a fila
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                    }
                    else
                    {
                        // Rejeitar definitivamente
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                        _logger.LogError("Mensagem rejeitada definitivamente após {RetryCount} tentativas", retryCount);
                    }
                }
            };

            await _channel.BasicConsumeAsync(
                queue: _settings.EmailQueueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Consumidor iniciado para a fila: {QueueName}", _settings.EmailQueueName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao iniciar consumidor da fila");
            return false;
        }
    }

    private int GetRetryCount(IReadOnlyBasicProperties properties)
    {
        if (properties.Headers != null && properties.Headers.TryGetValue("x-retry-count", out var value))
        {
            if (value is byte[] bytes)
            {
                var retryCountStr = Encoding.UTF8.GetString(bytes);
                if (int.TryParse(retryCountStr, out var retryCount))
                {
                    return retryCount;
                }
            }
        }
        return 0;
    }

    public void Dispose()
    {
        try
        {
            _channel?.Dispose();
            _connection?.Dispose();
            _logger.LogInformation("RabbitMQ desconectado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao desconectar do RabbitMQ");
        }
    }
}
