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

## Scripts de Configuração

### Script Principal: `setup.ps1`

Execute o script principal para configurar todo o ambiente:

```powershell
.\setup.ps1
```

**O script irá:**
- ✅ Verificar se o Docker está instalado e configurado
- ✅ Configurar o PATH do Docker se necessário (instalação via Microsoft Store)
- ✅ Fazer o deploy dos containers (RabbitMQ + MailHog)
- ✅ Executar as migrations do Entity Framework
- ✅ Verificar o status dos serviços

> **💡 Dica:** Para mais informações sobre os scripts disponíveis, consulte [`SCRIPTS_INFO.md`](SCRIPTS_INFO.md)

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

### Variáveis de Ambiente

#### Para Desenvolvimento (MailHog)

```bash
# Banco de Dados
DB_HOST=localhost
DB_NAME=msemail
DB_USER=postgres
DB_PASSWORD=admin

# SMTP (MailHog para desenvolvimento)
SMTP_HOST=localhost
SMTP_PORT=1025
SMTP_SSL=false
SMTP_USERNAME=
SMTP_PASSWORD=
SMTP_FROM_EMAIL=sistema@msemail.local
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

#### Para Produção (Gmail)

```bash
# Banco de Dados
DB_HOST=localhost
DB_NAME=msemail
DB_USER=postgres
DB_PASSWORD=sua_senha

# SMTP (Gmail para produção)
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
    "DefaultConnection": "Host=localhost;Database=msemail;Username=postgres;Password=admin"
  },
  "SmtpSettings": {
    "Host": "localhost",
    "Port": 1025,
    "EnableSsl": false,
    "Username": "",
    "Password": "",
    "FromEmail": "sistema@msemail.local",
    "FromName": "Sistema de Faturas"
  }
}
```

**Nota:** A configuração acima usa o MailHog para simular o envio de e-mails durante o desenvolvimento. Todos os e-mails enviados podem ser visualizados na interface web do MailHog em http://localhost:8025.

### MailHog - Simulador de E-mail para Desenvolvimento

O MailHog é uma ferramenta que simula um servidor SMTP para desenvolvimento, capturando todos os e-mails enviados e disponibilizando uma interface web para visualização.

**Características:**
- Interface web amigável para visualizar e-mails
- Não envia e-mails reais (seguro para desenvolvimento)
- API REST para automação de testes
- Suporte a anexos e e-mails HTML

**Como usar:**
1. Execute o container do MailHog: `docker-compose -f docker-compose.dev.yml up -d`
2. Configure a aplicação para usar `localhost:1025` como servidor SMTP
3. Acesse http://localhost:8025 para ver os e-mails enviados

**URLs importantes:**
- **Interface Web**: http://localhost:8025
- **Servidor SMTP**: localhost:1025
- **API REST**: http://localhost:8025/api/v1/messages

## Como Executar

### Opção 1: Setup Automático (Recomendado)

Execute o script de setup que configura tudo automaticamente:

```powershell
# Execute no PowerShell como Administrador
.\setup.ps1
```

O script irá:
- ✅ Verificar se Docker e .NET 8 estão instalados
- ✅ Configurar Docker automaticamente (se necessário)
- ✅ Subir containers do RabbitMQ e MailHog
- ✅ Instalar EF Core Tools (se necessário)
- ✅ Executar as migrações do banco
- ✅ Criar diretórios necessários

**Se o Docker não funcionar**, execute primeiro:
```powershell
# Configure o Docker no PATH (apenas uma vez)
.\configure-docker.ps1
```

### Opção 2: Setup Manual

#### 1. Preparar o Ambiente

#### Opção 1: Usando Docker Compose (Recomendado para Desenvolvimento)

Para subir apenas os serviços necessários (RabbitMQ e MailHog) para desenvolvimento local:

```bash
# Subir RabbitMQ e MailHog
docker-compose -f docker-compose.dev.yml up -d

# Verificar se os containers estão rodando
docker ps
```

**Serviços disponíveis:**
- **RabbitMQ Management**: http://localhost:15672 (usuário: `guest`, senha: `guest`)
- **MailHog Web UI**: http://localhost:8025 (interface para visualizar e-mails enviados)

#### Opção 2: Usando Docker individualmente

```bash
# Executar PostgreSQL (se não tiver instalado localmente)
docker run --name postgres-msemail -e POSTGRES_PASSWORD=admin -e POSTGRES_DB=msemail -p 5432:5432 -d postgres:15

# Executar RabbitMQ
docker run --name rabbitmq-msemail -p 5672:5672 -p 15672:15672 -d rabbitmq:3-management

# Executar MailHog para simular envio de e-mails
docker run --name mailhog-msemail -p 1025:1025 -p 8025:8025 -d mailhog/mailhog:v1.0.1
```

### 2. Configurar o Banco de Dados

Certifique-se de que o PostgreSQL está rodando e crie o banco de dados:

```sql
-- Conectar no PostgreSQL e criar o banco
CREATE DATABASE msemail;
```

### 3. Executar as Migrações

```bash
# Navegar para o projeto da API
cd MSEmail.API

# Executar as migrações do Entity Framework
dotnet ef database update
```

### 4. Executar a API

```bash
cd MSEmail.API
dotnet run
```

A API estará disponível em: https://localhost:7136 (Swagger na raiz)

### 5. Executar o Worker

```bash
cd MSEmail.Worker
dotnet run
```

### 6. Testar o Envio de E-mails

Depois que a aplicação estiver rodando, você pode:

1. Acessar o Swagger em: https://localhost:7136
2. Criar um template de e-mail
3. Enviar e-mails usando a API
4. Visualizar os e-mails enviados no MailHog: http://localhost:8025

**Exemplo de teste rápido:**
```bash
# Criar um template
curl -X POST "https://localhost:7136/api/emailtemplates" \
     -H "Content-Type: application/json" \
     -d '{
       "name": "Teste",
       "subject": "E-mail de Teste",
       "body": "<h1>Olá!</h1><p>Este é um e-mail de teste.</p>"
     }'

# Enviar e-mail (substitua os IDs pelos corretos)
curl -X POST "https://localhost:7136/api/emails/send" \
     -H "Content-Type: application/json" \
     -d '{
       "emailTemplateId": "GUID-DO-TEMPLATE",
       "recipientIds": ["GUID-DO-DESTINATARIO"]
     }'
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
