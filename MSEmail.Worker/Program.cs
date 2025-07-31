using MSEmail.Infrastructure.Extensions;
using MSEmail.Worker;

var builder = Host.CreateApplicationBuilder(args);

// Configurar logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configurar Entity Framework
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Adicionar infraestrutura
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddInfrastructureSettings(builder.Configuration);

// Registrar o worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
