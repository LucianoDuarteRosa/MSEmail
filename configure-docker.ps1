# Script para configurar o Docker no PATH (Uma única vez)
# Execute este script se o comando docker não funcionar no PowerShell

Write-Host "=== Configurando Docker no PATH do Sistema ===" -ForegroundColor Green

# Verificar se já está no PATH
if (Get-Command docker -ErrorAction SilentlyContinue) {
    Write-Host "✅ Docker já está disponível no PATH!" -ForegroundColor Green
    docker --version
    exit 0
}

# Caminho do Docker da Microsoft Store
$dockerPath = "C:\Program Files\Docker\Docker\resources\bin"

# Verificar se o Docker existe
if (-not (Test-Path "$dockerPath\docker.exe")) {
    Write-Host "❌ Docker não encontrado em $dockerPath" -ForegroundColor Red
    Write-Host "   Instale o Docker Desktop primeiro: https://www.docker.com/products/docker-desktop" -ForegroundColor Yellow
    exit 1
}

Write-Host "📁 Docker encontrado em: $dockerPath" -ForegroundColor Yellow

# Obter PATH atual do sistema
$currentPath = [Environment]::GetEnvironmentVariable("Path", [EnvironmentVariableTarget]::Machine)

# Verificar se o caminho já está no PATH
if ($currentPath -notlike "*$dockerPath*") {
    Write-Host "➕ Adicionando Docker ao PATH do sistema..." -ForegroundColor Yellow
    
    try {
        # Adicionar ao PATH do sistema (requer privilégios de administrador)
        $newPath = $currentPath + ";" + $dockerPath
        [Environment]::SetEnvironmentVariable("Path", $newPath, [EnvironmentVariableTarget]::Machine)
        
        Write-Host "✅ Docker adicionado ao PATH do sistema!" -ForegroundColor Green
        Write-Host "   Reinicie o PowerShell para que as mudanças tenham efeito." -ForegroundColor Yellow
        
    } catch {
        Write-Host "❌ Erro ao modificar PATH do sistema." -ForegroundColor Red
        Write-Host "   Execute este script como Administrador." -ForegroundColor Yellow
        
        # Adicionar apenas para a sessão atual
        Write-Host "   Adicionando para a sessão atual..." -ForegroundColor Yellow
        $env:PATH += ";$dockerPath"
        Write-Host "✅ Docker disponível para esta sessão!" -ForegroundColor Green
    }
} else {
    Write-Host "✅ Docker já está no PATH do sistema!" -ForegroundColor Green
}

# Testar Docker
Write-Host "`n🔍 Testando Docker..." -ForegroundColor Yellow
try {
    & "$dockerPath\docker.exe" --version
    Write-Host "✅ Docker funcionando corretamente!" -ForegroundColor Green
} catch {
    Write-Host "❌ Erro ao executar Docker." -ForegroundColor Red
}
