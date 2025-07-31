# Scripts de Configuração

## Scripts Ativos

### `setup.ps1` (PRINCIPAL)
Script unificado para configuração completa do ambiente de desenvolvimento.

**Funcionalidades:**
- ✅ Verificação automática do Docker
- ✅ Configuração do PATH se necessário
- ✅ Deploy dos containers (RabbitMQ + MailHog)
- ✅ Execução das migrations do Entity Framework
- ✅ Verificação do status dos serviços

**Como usar:**
```powershell
.\setup.ps1
```

### `configure-docker.ps1`
Script auxiliar para configuração do Docker PATH (usado automaticamente pelo setup.ps1 se necessário).

## Scripts Legados (Não usar)

### `setup-old.ps1`
Versão anterior do script de setup. **OBSOLETO** - usar `setup.ps1` no lugar.

### `setup.sh`
Script para sistemas Linux/Mac. **NÃO FUNCIONA NO WINDOWS** - usar `setup.ps1` no lugar.

## Recomendação

**Use apenas o `setup.ps1`** - ele é o script mais completo e atualizado.
