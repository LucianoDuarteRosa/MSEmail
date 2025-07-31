# Script de configuração para Windows PowerShell

Write-Host "=== Configurando Ambiente MSEmail ===" -ForegroundColor Green

# Criar diretório para arquivos
Write-Host "Criando diretório para arquivos..." -ForegroundColor Yellow
New-Item -ItemType Directory -Force -Path ".\files"

# Criar arquivo de teste
Write-Host "Criando arquivo de teste..." -ForegroundColor Yellow
"Este é um arquivo PDF de teste para o sistema MSEmail" | Out-File -FilePath ".\files\fatura_teste.pdf" -Encoding UTF8

# Verificar se Docker está instalado
if (Get-Command docker -ErrorAction SilentlyContinue) {
    Write-Host "Docker encontrado. Iniciando serviços de infraestrutura..." -ForegroundColor Yellow
    
    # Subir serviços de infraestrutura
    docker-compose up -d postgres rabbitmq
    
    # Aguardar serviços ficarem prontos
    Write-Host "Aguardando serviços ficarem prontos..." -ForegroundColor Yellow
    Start-Sleep 30
    
    Write-Host "Serviços de infraestrutura iniciados!" -ForegroundColor Green
} else {
    Write-Host "Docker não encontrado. Configure PostgreSQL e RabbitMQ manualmente." -ForegroundColor Red
}

# Verificar se EF Core Tools está instalado
$efToolsInstalled = dotnet tool list -g | Select-String "dotnet-ef"
if (-not $efToolsInstalled) {
    Write-Host "Instalando EF Core Tools..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
}

# Executar migrations
Write-Host "Executando migrations do banco de dados..." -ForegroundColor Yellow
Push-Location "MSEmail.API"
try {
    dotnet ef database update
    Write-Host "Migrations executadas com sucesso!" -ForegroundColor Green
} catch {
    Write-Host "Erro ao executar migrations. Verifique se o PostgreSQL está rodando." -ForegroundColor Red
}
Pop-Location

Write-Host "=== Ambiente Configurado com Sucesso! ===" -ForegroundColor Green
Write-Host ""
Write-Host "Serviços disponíveis:" -ForegroundColor Cyan
Write-Host "- PostgreSQL: localhost:5432" -ForegroundColor White
Write-Host "- RabbitMQ: localhost:5672 (Management: http://localhost:15672)" -ForegroundColor White
Write-Host "  - User: admin" -ForegroundColor Gray
Write-Host "  - Password: admin123" -ForegroundColor Gray
Write-Host ""
Write-Host "Para iniciar a aplicação:" -ForegroundColor Cyan
Write-Host "1. API: cd MSEmail.API; dotnet run" -ForegroundColor White
Write-Host "2. Worker: cd MSEmail.Worker; dotnet run" -ForegroundColor White
Write-Host ""
Write-Host "Swagger disponível em: https://localhost:7077" -ForegroundColor White
