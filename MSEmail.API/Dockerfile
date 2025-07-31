FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MSEmail.API/MSEmail.API.csproj", "MSEmail.API/"]
COPY ["MSEmail.Application/MSEmail.Application.csproj", "MSEmail.Application/"]
COPY ["MSEmail.Infrastructure/MSEmail.Infrastructure.csproj", "MSEmail.Infrastructure/"]
COPY ["MSEmail.Domain/MSEmail.Domain.csproj", "MSEmail.Domain/"]
RUN dotnet restore "MSEmail.API/MSEmail.API.csproj"

COPY . .
WORKDIR "/src/MSEmail.API"
RUN dotnet build "MSEmail.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MSEmail.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Criar diretório para arquivos
RUN mkdir -p /app/files

ENTRYPOINT ["dotnet", "MSEmail.API.dll"]
