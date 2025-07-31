## 1. Criar Template de E-mail

```http
POST http://localhost:5050/api/emailtemplates
Content-Type: application/json

{
  "name": "Fatura CDC",
  "subject": "Fatura disponível - CDC: {usuario.cdc}",
  "body": "<html><body><h1>Olá {usuario.nome}!</h1><p>Sua fatura do CDC: <strong>{usuario.cdc}</strong> está disponível.</p><p>Mês: {mes}</p><p>Ano: {ano}</p><p>Atenciosamente,<br/>Sistema de Faturas</p></body></html>"
}
```

## 2. Criar Destinatário

```http
POST http://localhost:5050/api/recipients
Content-Type: application/json

{
  "name": "João Silva",
  "email": "joao.silva@email.com",
  "cdc": "CDC001",
  "isActive": true
}
```

## 3. Listar Destinatários

```http
GET http://localhost:5050/api/recipients
```

## 4. Listar Apenas Destinatários Ativos

```http
GET http://localhost:5050/api/recipients?onlyActive=true
```

## 5. Obter Destinatário por ID

```http
GET http://localhost:5050/api/recipients/GUID-DO-DESTINATARIO
```

## 6. Obter Destinatário por E-mail

```http
GET http://localhost:5050/api/recipients/by-email/joao.silva@email.com
```

## 7. Atualizar Destinatário

```http
PUT http://localhost:5050/api/recipients/GUID-DO-DESTINATARIO
Content-Type: application/json

{
  "id": "GUID-DO-DESTINATARIO",
  "name": "João Silva Santos",
  "email": "joao.santos@email.com",
  "cdc": "CDC001",
  "isActive": true
}
```

## 8. Deletar Destinatário

```http
DELETE http://localhost:5050/api/recipients/GUID-DO-DESTINATARIO
```

## 9. Listar Templates

```http
GET http://localhost:5050/api/emailtemplates
```

## 10. Obter Template por Nome

```http
GET http://localhost:5050/api/emailtemplates/by-name/Fatura%20CDC
```

## 11. Enviar E-mails

```http
POST http://localhost:5050/api/emails/send
Content-Type: application/json

{
  "emailTemplateId": "GUID-DO-TEMPLATE",
  "recipientIds": ["GUID-DO-DESTINATARIO"],
  "additionalVariables": {
    "mes": "Janeiro",
    "ano": "2025"
  }
}
```

### Exemplo com PDF (opcional - validação desabilitada temporariamente)

```http
POST http://localhost:5050/api/emails/send
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

## 12. Consultar Estatísticas

```http
GET http://localhost:5000/api/emails/statistics
```

## 13. Reprocessar E-mail Falhado

```http
POST http://localhost:5000/api/emails/GUID-DO-EMAIL/reprocess
```

## 14. Listar Logs por Status

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
  curl -X POST http://localhost:5050/api/recipients \
    -H "Content-Type: application/json" \
    -d "{
      \"name\": \"Usuario $i\",
      \"email\": \"usuario$i@teste.com\",
      \"cdc\": \"CDC$i\",
      \"isActive\": true
    }"
done

# Enviar para todos
curl -X POST http://localhost:5050/api/emails/send \
  -H "Content-Type: application/json" \
  -d "{
    \"emailTemplateId\": \"GUID-DO-TEMPLATE\",
    \"recipientIds\": [\"LISTA-DE-GUIDS\"],
    \"pdfFileName\": \"fatura_teste.pdf\"
  }"
```

## Testando com MailHog

### 1. Verificar E-mails Enviados

Após enviar e-mails via API:

1. **Acesse a interface web**: http://localhost:8025
2. **Visualize os e-mails**: Todos os e-mails aparecerão na lista
3. **Clique para ver detalhes**: Assunto, corpo, anexos, etc.
4. **Teste variáveis**: Verifique se `{usuario.nome}`, `{usuario.cdc}` foram substituídos

### 2. API do MailHog

```bash
# Listar todos os e-mails
curl http://localhost:8025/api/v1/messages

# Obter e-mail específico
curl http://localhost:8025/api/v1/messages/{ID}

# Limpar todos os e-mails
curl -X DELETE http://localhost:8025/api/v1/messages
```

### 3. Monitoramento em Tempo Real

```bash
# Script para monitorar e-mails em tempo real
while true; do
  clear
  echo "=== E-mails no MailHog ==="
  curl -s http://localhost:8025/api/v1/messages | jq '.items | length'
  echo "Atualizado em: $(date)"
  sleep 5
done
```

## Configuração de Teste Local

Para testar localmente:

1. **Execute o setup automático:**
   ```powershell
   .\setup.ps1
   ```

2. **Ou configure manualmente:**
   ```bash
   # Subir RabbitMQ e MailHog
   docker-compose -f docker-compose.dev.yml up -d
   
   # Criar diretório para arquivos
   mkdir -p ./files
   ```

3. **Criar arquivo de teste PDF:**
   ```bash
   echo "Conteúdo de teste da fatura" > ./files/fatura_janeiro_2025.pdf
   ```

## URLs de Desenvolvimento

- **API**: http://localhost:5050
- **Swagger**: http://localhost:5050/swagger
- **MailHog Web**: http://localhost:8025
- **MailHog API**: http://localhost:8025/api/v1/messages
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)
