# Exemplos de Teste da API MSEmail

## 1. Criar Template de E-mail

```http
POST http://localhost:5000/api/emailtemplates
Content-Type: application/json

{
  "name": "Fatura CDC",
  "subject": "Fatura disponível - CDC: {usuario.cdc}",
  "body": "<html><body><h1>Olá {usuario.nome}!</h1><p>Sua fatura do CDC: <strong>{usuario.cdc}</strong> está disponível.</p><p>Mês: {mes}</p><p>Ano: {ano}</p><p>Atenciosamente,<br/>Sistema de Faturas</p></body></html>"
}
```

## 2. Criar Destinatário

```http
POST http://localhost:5000/api/recipients
Content-Type: application/json

{
  "name": "João Silva",
  "email": "joao.silva@email.com",
  "cdc": "CDC001",
  "isActive": true
}
```

## 3. Listar Templates

```http
GET http://localhost:5000/api/emailtemplates
```

## 4. Obter Template por Nome

```http
GET http://localhost:5000/api/emailtemplates/by-name/Fatura%20CDC
```

## 5. Enviar E-mails

```http
POST http://localhost:5000/api/emails/send
Content-Type: application/json

{
  "emailTemplateId": "GUID-DO-TEMPLATE",
  "recipientIds": ["GUID-DO-DESTINATARIO"],
  "pdfFileName": "fatura_janeiro_2025.pdf",
  "additionalVariables": {
    "mes": "Janeiro",
    "ano": "2025"
  }
}
```

## 6. Consultar Estatísticas

```http
GET http://localhost:5000/api/emails/statistics
```

## 7. Reprocessar E-mail Falhado

```http
POST http://localhost:5000/api/emails/GUID-DO-EMAIL/reprocess
```

## 8. Listar Logs por Status

```http
GET http://localhost:5000/api/emails/logs/by-status/Failed
```

## Exemplo de Resposta - Estatísticas

```json
{
  "totalEmails": 150,
  "pendingEmails": 5,
  "processingEmails": 2,
  "sentEmails": 140,
  "failedEmails": 3,
  "retryingEmails": 0,
  "generatedAt": "2025-01-31T10:30:00Z"
}
```

## Exemplo de Template com Variáveis

```html
<!DOCTYPE html>
<html>
<head>
    <title>Fatura Disponível</title>
</head>
<body>
    <h1>Olá {usuario.nome}!</h1>
    
    <p>Sua fatura do CDC <strong>{usuario.cdc}</strong> está disponível para o período de {mes}/{ano}.</p>
    
    <h2>Detalhes:</h2>
    <ul>
        <li><strong>Nome:</strong> {usuario.nome}</li>
        <li><strong>CDC:</strong> {usuario.cdc}</li>
        <li><strong>Período:</strong> {mes}/{ano}</li>
        <li><strong>Data de Envio:</strong> {dataEnvio}</li>
    </ul>
    
    <p>Em anexo você encontrará o arquivo PDF com todos os detalhes da sua fatura.</p>
    
    <p>Atenciosamente,<br/>
    <strong>Sistema de Faturas</strong></p>
</body>
</html>
```

## Teste de Carga de E-mails

```bash
# Criar múltiplos destinatários
for i in {1..100}; do
  curl -X POST http://localhost:5000/api/recipients \
    -H "Content-Type: application/json" \
    -d "{
      \"name\": \"Usuario $i\",
      \"email\": \"usuario$i@teste.com\",
      \"cdc\": \"CDC$i\",
      \"isActive\": true
    }"
done

# Enviar para todos
curl -X POST http://localhost:5000/api/emails/send \
  -H "Content-Type: application/json" \
  -d "{
    \"emailTemplateId\": \"GUID-DO-TEMPLATE\",
    \"recipientIds\": [\"LISTA-DE-GUIDS\"],
    \"pdfFileName\": \"fatura_teste.pdf\"
  }"
```

## Configuração de Teste Local

Para testar localmente, configure:

1. **PostgreSQL:**
   ```bash
   docker run --name postgres-test -e POSTGRES_PASSWORD=123456 -e POSTGRES_DB=msemail_test -p 5432:5432 -d postgres:15
   ```

2. **RabbitMQ:**
   ```bash
   docker run --name rabbitmq-test -p 5672:5672 -p 15672:15672 -d rabbitmq:3-management
   ```

3. **Criar arquivo de teste PDF:**
   ```bash
   mkdir -p c:\temp\msemail\files
   echo "Conteúdo de teste da fatura" > c:\temp\msemail\files\fatura_janeiro_2025.pdf
   ```

4. **Configurar SMTP (Gmail):**
   - Ativar verificação em 2 etapas
   - Gerar senha de app
   - Usar a senha de app no appsettings.json
