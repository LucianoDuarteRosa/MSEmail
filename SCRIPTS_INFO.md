# Scripts de Configuração

## Scripts Ativos

### `setup.ps1` (PRINCIPAL)
Script unificado para configuração completa do ambiente de desenvolvimento.

**Funcionalidades:**
- ✅ Verificação automática do Docker e .NET
- ✅ Configuração do PATH se necessário
- ✅ Deploy dos containers (RabbitMQ + MailHog)
- ✅ Execução das migrations do Entity Framework
- ✅ Inicialização automática da API e Worker em janelas separadas
- ✅ Verificação do status dos serviços

**Como usar:**
```powershell
.\setup.ps1
```

**O que acontece:**
- Para containers existentes e sobe novos
- Aguarda containers ficarem prontos
- Executa migrations do banco
- Abre 2 janelas PowerShell: uma para API e outra para Worker
- Mostra URLs dos serviços

### `configure-docker.ps1`
Script auxiliar para configuração do Docker PATH (usado automaticamente pelo setup.ps1 se necessário).


## Recomendação

**Use apenas o `setup.ps1`** - ele é o script mais completo e atualizado.
