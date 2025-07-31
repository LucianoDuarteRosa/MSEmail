using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MSEmail.Domain.Interfaces;
using MSEmail.Domain.Models;
using MSEmail.Infrastructure.Data;
using MSEmail.Infrastructure.Repositories;
using MSEmail.Infrastructure.Services;

namespace MSEmail.Infrastructure.Extensions;

/// <summary>
/// Extensões para configurar os serviços de infraestrutura
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // Configurar Entity Framework
        services.AddDbContext<MSEmailDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Registrar repositórios
        services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();
        services.AddScoped<IRecipientRepository, RecipientRepository>();
        services.AddScoped<IEmailLogRepository, EmailLogRepository>();

        // Registrar serviços
        services.AddScoped<IEmailService, EmailService>();
        services.AddSingleton<IMessageQueueService, RabbitMqService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        return services;
    }

    public static IServiceCollection AddInfrastructureSettings(this IServiceCollection services, IConfiguration configuration)
    {
        // Configurar settings de forma tipada
        services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.SectionName));
        services.Configure<RabbitMqSettings>(configuration.GetSection(RabbitMqSettings.SectionName));
        services.Configure<FileStorageSettings>(configuration.GetSection(FileStorageSettings.SectionName));

        return services;
    }
}
