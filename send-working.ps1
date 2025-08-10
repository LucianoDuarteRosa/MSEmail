# Script corrigido para envio de e-mails
$baseUrl = "http://localhost:5050"
$templateId = "f3bf5e4d-639f-4e9c-b2b8-16b48934f090"
$recipientId = "730f5f59-40ac-4c5e-b326-b9348bf6e5b1"

Write-Host "Enviando 1000 e-mails para a fila..." -ForegroundColor Green

for ($i = 1; $i -le 1000; $i++) {
    # Corrigido: todas as variáveis como string
    $body = @{
        emailTemplateId = $templateId
        recipientIds = @($recipientId)
        additionalVariables = @{
            mes = "Janeiro"
            ano = "2025"
            numero = "$i"  # Convertido para string
            lote = "teste-$i"
        }
    }

    $jsonBody = $body | ConvertTo-Json -Depth 3

    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/api/emails/send" -Method POST -Body $jsonBody -ContentType "application/json"
        Write-Host "E-mail $i enviado: $($response.message)" -ForegroundColor Green
    }
    catch {
        Write-Host "Erro no e-mail $i`: $($_.Exception.Message)" -ForegroundColor Red
    }
    
    Start-Sleep -Milliseconds 200
}

Write-Host "`nScript concluido! Verifique:" -ForegroundColor Yellow
Write-Host "- RabbitMQ Management: http://localhost:15672" -ForegroundColor Cyan
Write-Host "- MailHog: http://localhost:8025" -ForegroundColor Cyan
Write-Host "- API Stats: http://localhost:5050/api/emails/statistics" -ForegroundColor Cyan
