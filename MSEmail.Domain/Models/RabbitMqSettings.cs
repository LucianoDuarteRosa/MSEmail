namespace MSEmail.Domain.Models;

/// <summary>
/// Configurações para RabbitMQ
/// </summary>
public class RabbitMqSettings
{
    public const string SectionName = "RabbitMqSettings";

    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string EmailQueueName { get; set; } = "email-queue";
    public string EmailExchangeName { get; set; } = "email-exchange";
    public string EmailRoutingKey { get; set; } = "email.send";
    public bool Durable { get; set; } = true;
    public int MaxRetryAttempts { get; set; } = 3;
    public int RetryDelayInSeconds { get; set; } = 60;
}
