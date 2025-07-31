#!/bin/bash

echo "=== Configurando Ambiente MSEmail ==="

# Criar diretório para arquivos
echo "Criando diretório para arquivos..."
mkdir -p ./files

# Criar arquivo de teste
echo "Criando arquivo de teste..."
echo "Este é um arquivo PDF de teste para o sistema MSEmail" > ./files/fatura_teste.pdf

# Subir serviços de infraestrutura
echo "Iniciando PostgreSQL e RabbitMQ..."
docker-compose up -d postgres rabbitmq

# Aguardar serviços ficarem prontos
echo "Aguardando serviços ficarem prontos..."
sleep 30

# Executar migrations
echo "Executando migrations do banco de dados..."
cd MSEmail.API
dotnet ef database update
cd ..

echo "=== Ambiente Configurado com Sucesso! ==="
echo ""
echo "Serviços disponíveis:"
echo "- PostgreSQL: localhost:5432"
echo "- RabbitMQ: localhost:5672 (Management: http://localhost:15672)"
echo "  - User: admin"
echo "  - Password: admin123"
echo ""
echo "Para iniciar a aplicação:"
echo "1. API: cd MSEmail.API && dotnet run"
echo "2. Worker: cd MSEmail.Worker && dotnet run"
echo ""
echo "Swagger disponível em: https://localhost:7077"
