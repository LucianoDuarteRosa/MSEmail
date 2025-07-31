# MSEmail - Script de Setup Completo
# Este script configura todo o ambiente de desenvolvimento automaticamente

Write-Host "=== MSEmail - Setup Completo ===" -ForegroundColor Green
Write-Host ""

# Função para verificar se um comando existe
function Test-Command {
    param($Command)
    try {
        Get-Command $Command -ErrorAction Stop
        return $true
    }
    catch {
        return $false
    }
}

# Verificar Docker
Write-Host "📦 Verificando Docker..." -ForegroundColor Yellow
if (-not (Test-Command "docker")) {
    Write-Host "❌ Docker não encontrado!" -ForegroundColor Red
    Write-Host "💡 Executando configuração do Docker..." -ForegroundColor Cyan
    
    if (Test-Path ".\configure-docker.ps1") {
        & ".\configure-docker.ps1"
        Write-Host "🔄 Reinicie o PowerShell e execute novamente o setup.ps1" -ForegroundColor Yellow
        Read-Host "Pressione Enter para sair"
        exit
    } else {
        Write-Host "❌ configure-docker.ps1 não encontrado!" -ForegroundColor Red
        Write-Host "💡 Instale o Docker Desktop e reinicie o PowerShell" -ForegroundColor Cyan
        Read-Host "Pressione Enter para sair"
        exit
    }
}

Write-Host "✅ Docker encontrado!" -ForegroundColor Green

# Verificar .NET
Write-Host "🔧 Verificando .NET..." -ForegroundColor Yellow
if (-not (Test-Command "dotnet")) {
    Write-Host "❌ .NET não encontrado! Instale o .NET 8 SDK" -ForegroundColor Red
    Read-Host "Pressione Enter para sair"
    exit
}

$dotnetVersion = dotnet --version
Write-Host "✅ .NET $dotnetVersion encontrado!" -ForegroundColor Green

# Parar containers existentes (se houver)
Write-Host ""
Write-Host "🛑 Parando containers existentes..." -ForegroundColor Yellow
docker-compose -f docker-compose.dev.yml down 2>$null

# Subir containers de desenvolvimento
Write-Host ""
Write-Host "🚀 Subindo containers (RabbitMQ + MailHog)..." -ForegroundColor Yellow
docker-compose -f docker-compose.dev.yml up -d

# Aguardar containers ficarem prontos
Write-Host ""
Write-Host "⏳ Aguardando containers ficarem prontos..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

# Verificar status dos containers
Write-Host ""
Write-Host "📋 Status dos containers:" -ForegroundColor Yellow
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | Where-Object { $_ -match "rabbitmq|mailhog" }

# Executar migrations
Write-Host ""
Write-Host "🗄️ Verificando e executando migrations..." -ForegroundColor Yellow
Push-Location "MSEmail.API"
try {
    dotnet ef database update
    Write-Host "✅ Migrations executadas com sucesso!" -ForegroundColor Green
}
catch {
    Write-Host "❌ Erro ao executar migrations: $_" -ForegroundColor Red
}
finally {
    Pop-Location
}

# Função para iniciar aplicação em nova janela
function Start-AppInNewWindow {
    param(
        [string]$Path,
        [string]$Title,
        [string]$Command
    )
    
    $scriptBlock = {
        param($path, $title, $cmd)
        Set-Location $path
        $Host.UI.RawUI.WindowTitle = $title
        Write-Host "=== $title ===" -ForegroundColor Green
        Write-Host "📍 Diretório: $path" -ForegroundColor Cyan
        Write-Host "🚀 Executando: $cmd" -ForegroundColor Yellow
        Write-Host ""
        Invoke-Expression $cmd
    }
    
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "& {$scriptBlock} '$Path' '$Title' '$Command'"
}

Write-Host ""
Write-Host "🎯 Iniciando aplicações..." -ForegroundColor Green

# Iniciar API em nova janela
Write-Host "🌐 Iniciando API..." -ForegroundColor Yellow
$apiPath = Join-Path $PWD "MSEmail.API"
Start-AppInNewWindow -Path $apiPath -Title "MSEmail API" -Command "dotnet run"

# Aguardar um pouco antes de iniciar o Worker
Start-Sleep -Seconds 3

# Iniciar Worker em nova janela
Write-Host "⚙️ Iniciando Worker..." -ForegroundColor Yellow
$workerPath = Join-Path $PWD "MSEmail.Worker"
Start-AppInNewWindow -Path $workerPath -Title "MSEmail Worker" -Command "dotnet run"

# Resumo final
Write-Host ""
Write-Host "🎉 Setup concluído com sucesso!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Serviços disponíveis:" -ForegroundColor Cyan
Write-Host "  🌐 API Swagger: https://localhost:7136" -ForegroundColor White
Write-Host "  📧 MailHog UI: http://localhost:8025" -ForegroundColor White
Write-Host "  🐰 RabbitMQ Management: http://localhost:15672 (guest/guest)" -ForegroundColor White
Write-Host ""
Write-Host "💡 Duas novas janelas do PowerShell foram abertas:" -ForegroundColor Yellow
Write-Host "  - MSEmail API (porta 7136)" -ForegroundColor White
Write-Host "  - MSEmail Worker (processamento de emails)" -ForegroundColor White
Write-Host ""
Write-Host "🔧 Para parar os containers:" -ForegroundColor Cyan
Write-Host "  docker-compose -f docker-compose.dev.yml down" -ForegroundColor White
Write-Host ""

Read-Host "Pressione Enter para fechar esta janela"
