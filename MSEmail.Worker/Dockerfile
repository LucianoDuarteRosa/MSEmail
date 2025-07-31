FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MSEmail.Worker/MSEmail.Worker.csproj", "MSEmail.Worker/"]
COPY ["MSEmail.Application/MSEmail.Application.csproj", "MSEmail.Application/"]
COPY ["MSEmail.Infrastructure/MSEmail.Infrastructure.csproj", "MSEmail.Infrastructure/"]
COPY ["MSEmail.Domain/MSEmail.Domain.csproj", "MSEmail.Domain/"]
RUN dotnet restore "MSEmail.Worker/MSEmail.Worker.csproj"

COPY . .
WORKDIR "/src/MSEmail.Worker"
RUN dotnet build "MSEmail.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MSEmail.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Criar diretório para arquivos
RUN mkdir -p /app/files

ENTRYPOINT ["dotnet", "MSEmail.Worker.dll"]
