# Script para configurar o ambiente de desenvolvimento do MSEmail
# Execute este script no PowerShell

Write-Host "===================================" -ForegroundColor Green
Write-Host "MSEmail - Setup de Desenvolvimento" -ForegroundColor Green
Write-Host "===================================" -ForegroundColor Green

# Função para verificar se um comando existe
function Test-Command($command) {
    try {
        if (Get-Command $command -ErrorAction Stop) { return $true }
    } catch { return $false }
}

# Verificar se o Docker está instalado e funcionando
Write-Host "`n1. Verificando Docker..." -ForegroundColor Yellow
$dockerAvailable = $false

# Primeiro, tentar o comando docker padrão
try {
    $dockerVersion = docker --version 2>$null
    if ($LASTEXITCODE -eq 0 -and $dockerVersion) {
        Write-Host "✅ Docker encontrado: $dockerVersion" -ForegroundColor Green
        
        # Verificar se o Docker está rodando
        Write-Host "   Verificando se Docker está rodando..." -ForegroundColor Gray
        $dockerInfo = docker info 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Docker está rodando corretamente!" -ForegroundColor Green
            $dockerAvailable = $true
        } else {
            Write-Host "⚠️  Docker encontrado mas não está rodando." -ForegroundColor Yellow
            Write-Host "   Por favor, inicie o Docker Desktop e execute o script novamente." -ForegroundColor Yellow
            Write-Host "   Ou continue manualmente sem Docker." -ForegroundColor Yellow
            $dockerAvailable = $false
        }
    } else {
        throw "Docker command failed"
    }
} catch {
    # Tentar encontrar Docker em locais alternativos (Microsoft Store, etc.)
    Write-Host "⚠️  Docker não encontrado no PATH padrão" -ForegroundColor Yellow
    Write-Host "   Verificando instalações alternativas..." -ForegroundColor Yellow
    
    $alternativePaths = @(
        "$env:LOCALAPPDATA\Microsoft\WindowsApps\docker.exe",
        "$env:ProgramFiles\Docker\Docker\resources\bin\docker.exe",
        "$env:ProgramFiles\Docker\Docker\Docker Desktop.exe"
    )
    
    $dockerFound = $false
    foreach ($path in $alternativePaths) {
        if (Test-Path $path) {
            Write-Host "✅ Docker encontrado em: $path" -ForegroundColor Green
            $dockerFound = $true
            
            # Tentar usar este path
            try {
                $env:PATH += ";$(Split-Path $path)"
                $dockerVersion = & $path --version 2>$null
                if ($dockerVersion) {
                    Write-Host "✅ Versão: $dockerVersion" -ForegroundColor Green
                    $dockerAvailable = $true
                }
            } catch {
                Write-Host "⚠️  Docker encontrado mas não está funcionando" -ForegroundColor Yellow
            }
            break
        }
    }
    
    if (-not $dockerFound) {
        Write-Host "❌ Docker não encontrado em nenhum local conhecido" -ForegroundColor Red
        Write-Host "   Certifique-se de que o Docker Desktop está instalado e rodando" -ForegroundColor Yellow
        Write-Host "   Download: https://www.docker.com/products/docker-desktop" -ForegroundColor Yellow
    }
}

# Verificar se o .NET 8 está instalado
Write-Host "`n2. Verificando .NET 8..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version 2>$null
    if ($LASTEXITCODE -eq 0 -and $dotnetVersion) {
        if ($dotnetVersion -like "8.*") {
            Write-Host "✅ .NET 8 encontrado: $dotnetVersion" -ForegroundColor Green
        } else {
            Write-Host "⚠️  .NET encontrado mas não é versão 8: $dotnetVersion" -ForegroundColor Yellow
            Write-Host "   Recomendamos instalar o .NET 8 SDK" -ForegroundColor Yellow
            Write-Host "   Download: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
        }
    } else {
        throw ".NET não encontrado"
    }
} catch {
    Write-Host "❌ .NET não encontrado. Instale o .NET 8 SDK primeiro." -ForegroundColor Red
    Write-Host "   Download: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
    
    $continue = Read-Host "`nDeseja continuar sem verificar o .NET? (s/N)"
    if ($continue -ne "s" -and $continue -ne "S") {
        exit 1
    }
}

# Configurar containers de desenvolvimento
if ($dockerAvailable) {
    Write-Host "`n3. Subindo containers (RabbitMQ e MailHog)..." -ForegroundColor Yellow
    try {
        docker-compose -f docker-compose.dev.yml up -d
        Write-Host "✅ Containers iniciados com sucesso!" -ForegroundColor Green
        
        # Aguardar containers estarem prontos
        Write-Host "`n4. Aguardando containers ficarem prontos..." -ForegroundColor Yellow
        Start-Sleep -Seconds 10
        
        # Verificar status dos containers
        Write-Host "`n5. Verificando status dos containers..." -ForegroundColor Yellow
        try {
            $containers = docker ps --filter "name=msemail" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
            Write-Host $containers -ForegroundColor White
        } catch {
            Write-Host "⚠️  Não foi possível verificar status dos containers" -ForegroundColor Yellow
        }
        
    } catch {
        Write-Host "❌ Erro ao iniciar containers." -ForegroundColor Red
        Write-Host "   Erro: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "`n⚠️  Continuando sem containers - configure RabbitMQ e MailHog manualmente" -ForegroundColor Yellow
        $dockerAvailable = $false
    }
} else {
    Write-Host "`n3. ⚠️  Pulando configuração de containers (Docker não disponível)" -ForegroundColor Yellow
    Write-Host "`n   CONFIGURAÇÃO MANUAL NECESSÁRIA:" -ForegroundColor Cyan
    Write-Host "   1. Instale e configure RabbitMQ:" -ForegroundColor White
    Write-Host "      - Download: https://www.rabbitmq.com/download.html" -ForegroundColor Gray
    Write-Host "      - Ou use: chocolatey install rabbitmq" -ForegroundColor Gray
    Write-Host "   2. Instale e configure MailHog:" -ForegroundColor White
    Write-Host "      - Download: https://github.com/mailhog/MailHog/releases" -ForegroundColor Gray
    Write-Host "      - Ou use: chocolatey install mailhog" -ForegroundColor Gray
}

# Verificar/Instalar EF Core Tools
Write-Host "`n6. Verificando EF Core Tools..." -ForegroundColor Yellow
try {
    $efToolsInstalled = dotnet tool list -g | Select-String "dotnet-ef" 2>$null
    if ($efToolsInstalled) {
        Write-Host "✅ EF Core Tools já está instalado!" -ForegroundColor Green
    } else {
        Write-Host "📦 Instalando EF Core Tools..." -ForegroundColor Yellow
        dotnet tool install --global dotnet-ef
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ EF Core Tools instalado com sucesso!" -ForegroundColor Green
        } else {
            Write-Host "❌ Erro ao instalar EF Core Tools" -ForegroundColor Red
        }
    }
} catch {
    Write-Host "⚠️  Não foi possível verificar/instalar EF Core Tools" -ForegroundColor Yellow
}

# Executar migrações do banco
Write-Host "`n7. Executando migrações do banco de dados..." -ForegroundColor Yellow
if (Test-Path "MSEmail.API") {
    try {
        Set-Location "MSEmail.API"
        dotnet ef database update
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Migrações executadas com sucesso!" -ForegroundColor Green
        } else {
            Write-Host "❌ Erro ao executar migrações." -ForegroundColor Red
            Write-Host "   Certifique-se de que o PostgreSQL está rodando com as configurações corretas." -ForegroundColor Yellow
        }
        Set-Location ".."
    } catch {
        Write-Host "❌ Erro ao executar migrações: $($_.Exception.Message)" -ForegroundColor Red
        Set-Location ".."
    }
} else {
    Write-Host "❌ Diretório MSEmail.API não encontrado" -ForegroundColor Red
}

# Criar diretório para arquivos
Write-Host "`n8. Criando diretório para arquivos..." -ForegroundColor Yellow
try {
    $filesDir = ".\files"
    if (-not (Test-Path $filesDir)) {
        New-Item -ItemType Directory -Force -Path $filesDir | Out-Null
        Write-Host "✅ Diretório '$filesDir' criado!" -ForegroundColor Green
    } else {
        Write-Host "✅ Diretório '$filesDir' já existe!" -ForegroundColor Green
    }
} catch {
    Write-Host "⚠️  Erro ao criar diretório: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Resumo final
Write-Host "`n===================================" -ForegroundColor Green
Write-Host "Setup concluído!" -ForegroundColor Green
Write-Host "===================================" -ForegroundColor Green

if ($dockerAvailable) {
    Write-Host "`n✅ Serviços Docker disponíveis:" -ForegroundColor Cyan
    Write-Host "   • RabbitMQ Management: http://localhost:15672 (guest/guest)" -ForegroundColor White
    Write-Host "   • MailHog Web UI: http://localhost:8025" -ForegroundColor White
} else {
    Write-Host "`n⚠️  Serviços Docker não disponíveis - configuração manual necessária" -ForegroundColor Yellow
}

Write-Host "`n📋 Próximos passos:" -ForegroundColor Cyan
Write-Host "   1. Execute a API: cd MSEmail.API && dotnet run" -ForegroundColor White
Write-Host "   2. Execute o Worker: cd MSEmail.Worker && dotnet run" -ForegroundColor White
Write-Host "   3. Acesse o Swagger: https://localhost:7077" -ForegroundColor White
if ($dockerAvailable) {
    Write-Host "   4. Visualize e-mails no MailHog: http://localhost:8025" -ForegroundColor White
}

Write-Host "`n🛠️  Comandos úteis:" -ForegroundColor Cyan
if ($dockerAvailable) {
    Write-Host "   • Parar containers: docker-compose -f docker-compose.dev.yml down" -ForegroundColor White
    Write-Host "   • Ver logs: docker-compose -f docker-compose.dev.yml logs -f" -ForegroundColor White
}
Write-Host "   • Ver status da API: dotnet run --project MSEmail.API" -ForegroundColor White
Write-Host "   • Ver exemplos de uso: Consulte API_EXAMPLES.md" -ForegroundColor White

Write-Host "`n🎉 Ambiente configurado! Boa sorte com o desenvolvimento!" -ForegroundColor Green
