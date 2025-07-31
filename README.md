# MSEmail - Microserviço de Envio de Faturas por E-mail

## Descrição

MSEmail é um microserviço completo desenvolvido em .NET 8 para envio automático de faturas por e-mail. O sistema utiliza arquitetura DDD (Domain-Driven Design) com padrões CQRS e inclui funcionalidades avançadas como:

- ✅ Fila de processamento com RabbitMQ
- ✅ Reprocessamento automático com até 3 tentativas
- ✅ Templates de mensagem configuráveis
- ✅ Anexo de PDF das faturas
- ✅ APIs RESTful para administração
- ✅ Worker para processamento assíncrono
- ✅ Logging detalhado
- ✅ Configuração por variáveis de ambiente

## Arquitetura

O projeto segue a arquitetura Clean Architecture com as seguintes camadas:

```
MSEmail/
├── MSEmail.API/             # Camada de apresentação (Controllers)
├── MSEmail.Application/     # Camada de aplicação (Handlers, DTOs, Commands/Queries)
├── MSEmail.Domain/          # Camada de domínio (Entidades, Interfaces, Models)
├── MSEmail.Infrastructure/  # Camada de infraestrutura (Repositórios, Serviços, EF)
└── MSEmail.Worker/          # Worker para processamento de fila
```

## Tecnologias Utilizadas

- **Framework**: .NET 8
- **ORM**: Entity Framework Core
- **Banco de Dados**: PostgreSQL
- **Fila de Mensagens**: RabbitMQ
- **E-mail**: MailKit
- **Mapeamento**: AutoMapper
- **Mediator**: MediatR
- **Documentação**: Swagger/OpenAPI

## Funcionalidades Principais

### 1. Fila de Processamento
- Consulta destinatários aptos no banco de dados
- Envia dados para fila RabbitMQ
- Processamento assíncrono pelo Worker

### 2. Reprocessamento Automático
- Até 3 tentativas de envio por e-mail
- Delay configurável entre tentativas
- Log detalhado de falhas

### 3. Templates de Mensagem
- Templates armazenados no banco de dados
- Variáveis substituíveis (ex: `{usuario.nome}`, `{usuario.cdc}`)
- CRUD completo via API

### 4. Anexo de PDF
- Suporte a anexos de fatura
- Armazenamento local ou blob storage
- Validação de existência do arquivo

### 5. APIs Administrativas
- Gerenciamento de templates (CRUD)
- Consulta de status de envios
- Reprocessamento manual
- Estatísticas de envio

## Configuração

### Requisitos
- .NET 8 SDK
- PostgreSQL
- RabbitMQ

### Variáveis de Ambiente (Produção)

```bash
# Banco de Dados
DB_HOST=localhost
DB_NAME=msemail
DB_USER=postgres
DB_PASSWORD=sua_senha

# SMTP
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_SSL=true
SMTP_USERNAME=seu_email@gmail.com
SMTP_PASSWORD=sua_senha_de_app
SMTP_FROM_EMAIL=seu_email@gmail.com
SMTP_FROM_NAME="Sistema de Faturas"

# RabbitMQ
RABBITMQ_HOST=localhost
RABBITMQ_PORT=5672
RABBITMQ_USER=guest
RABBITMQ_PASSWORD=guest

# Armazenamento
STORAGE_TYPE=Local
STORAGE_PATH=/app/files
```

### Configuração de Desenvolvimento

Edite os arquivos `appsettings.Development.json` em cada projeto:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=msemail_dev;Username=postgres;Password=123456"
  },
  "SmtpSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "EnableSsl": true,
    "Username": "seu_email@gmail.com",
    "Password": "sua_senha_de_app",
    "FromEmail": "seu_email@gmail.com",
    "FromName": "Sistema de Faturas"
  }
}
```

## Como Executar

### 1. Preparar o Ambiente

```bash
# Executar PostgreSQL (Docker)
docker run --name postgres-msemail -e POSTGRES_PASSWORD=123456 -e POSTGRES_DB=msemail_dev -p 5432:5432 -d postgres:15

# Executar RabbitMQ (Docker)
docker run --name rabbitmq-msemail -p 5672:5672 -p 15672:15672 -d rabbitmq:3-management
```

### 2. Executar a API

```bash
cd MSEmail.API
dotnet run
```

A API estará disponível em: https://localhost:7077 (Swagger na raiz)

### 3. Executar o Worker

```bash
cd MSEmail.Worker
dotnet run
```

## Endpoints da API

### Templates de E-mail

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/emailtemplates` | Lista todos os templates |
| GET | `/api/emailtemplates/{id}` | Obtém template por ID |
| GET | `/api/emailtemplates/by-name/{name}` | Obtém template por nome |
| POST | `/api/emailtemplates` | Cria novo template |
| PUT | `/api/emailtemplates/{id}` | Atualiza template |
| DELETE | `/api/emailtemplates/{id}` | Remove template |

### Gerenciamento de E-mails

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/api/emails/send` | Envia e-mails para múltiplos destinatários |
| POST | `/api/emails/{id}/reprocess` | Reprocessa e-mail específico |
| GET | `/api/emails/logs/{id}` | Obtém log de e-mail |
| GET | `/api/emails/logs/by-status/{status}` | Lista logs por status |
| GET | `/api/emails/statistics` | Estatísticas de envio |

## Exemplos de Uso

### 1. Criar Template de E-mail

```bash
POST /api/emailtemplates
Content-Type: application/json

{
  "name": "Fatura CDC",
  "subject": "Fatura disponível - CDC: {usuario.cdc}",
  "body": "<h1>Olá {usuario.nome}!</h1><p>Sua fatura do CDC: {usuario.cdc} está disponível.</p>"
}
```

### 2. Enviar E-mails

```bash
POST /api/emails/send
Content-Type: application/json

{
  "emailTemplateId": "guid-do-template",
  "recipientIds": ["guid-destinatario-1", "guid-destinatario-2"],
  "pdfFileName": "fatura_janeiro_2025.pdf",
  "additionalVariables": {
    "mes": "Janeiro",
    "ano": "2025"
  }
}
```

### 3. Consultar Estatísticas

```bash
GET /api/emails/statistics

Resposta:
{
  "totalEmails": 1500,
  "pendingEmails": 10,
  "processingEmails": 5,
  "sentEmails": 1450,
  "failedEmails": 35,
  "retryingEmails": 0,
  "generatedAt": "2025-01-31T10:30:00Z"
}
```

## Estrutura do Banco de Dados

### EmailTemplates
- Id (GUID)
- Name (string, único)
- Subject (string)
- Body (text)
- IsActive (bool)
- CreatedAt, UpdatedAt (datetime)

### Recipients
- Id (GUID)
- Name (string)
- Email (string, único)
- Cdc (string)
- IsActive (bool)
- CreatedAt, UpdatedAt (datetime)

### EmailLogs
- Id (GUID)
- RecipientId (GUID, FK)
- EmailTemplateId (GUID, FK)
- Subject (string)
- Body (text)
- Status (enum: Pending, Processing, Sent, Failed, Retrying)
- AttemptCount (int)
- MaxAttempts (int)
- ErrorMessage (string, nullable)
- SentAt (datetime, nullable)
- CreatedAt, UpdatedAt (datetime)

## Padrões Implementados

- **Domain-Driven Design (DDD)**
- **Command Query Responsibility Segregation (CQRS)**
- **Repository Pattern**
- **Dependency Injection**
- **Options Pattern**
- **Background Services**
- **Entity Framework Code First**

## Logs e Monitoramento

O sistema gera logs detalhados para:
- ✅ Criação e processamento de e-mails
- ✅ Falhas e reprocessamentos
- ✅ Operações no banco de dados
- ✅ Conexões com RabbitMQ e SMTP
- ✅ Performance e timing

## Segurança

- ✅ Validação de entrada nos DTOs
- ✅ Prevenção de path traversal no file storage
- ✅ Timeout configurável para SMTP
- ✅ Configuração segura de credenciais via variáveis de ambiente

## Deploy

### Docker

```dockerfile
# Exemplo de Dockerfile para a API
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MSEmail.API/MSEmail.API.csproj", "MSEmail.API/"]
COPY ["MSEmail.Application/MSEmail.Application.csproj", "MSEmail.Application/"]
COPY ["MSEmail.Infrastructure/MSEmail.Infrastructure.csproj", "MSEmail.Infrastructure/"]
COPY ["MSEmail.Domain/MSEmail.Domain.csproj", "MSEmail.Domain/"]
RUN dotnet restore "MSEmail.API/MSEmail.API.csproj"

COPY . .
WORKDIR "/src/MSEmail.API"
RUN dotnet publish "MSEmail.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MSEmail.API.dll"]
```

## Contribuição

1. Faça um fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/nova-feature`)
3. Commit suas mudanças (`git commit -am 'Adiciona nova feature'`)
4. Push para a branch (`git push origin feature/nova-feature`)
5. Abra um Pull Request

## Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

## Suporte

Para dúvidas ou problemas:
- Abra uma issue no GitHub
- Consulte a documentação da API via Swagger
- Verifique os logs da aplicação
