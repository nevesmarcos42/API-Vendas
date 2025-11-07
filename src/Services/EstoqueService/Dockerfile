# Dockerfile para o Serviço de Estoque
# Utiliza imagem base do .NET 8 para aplicações ASP.NET Core

# Estágio de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia arquivos de projeto para restaurar dependências
COPY ["src/Services/EstoqueService/EstoqueService.csproj", "src/Services/EstoqueService/"]
COPY ["src/Shared/SharedLibrary/SharedLibrary.csproj", "src/Shared/SharedLibrary/"]

# Restaura as dependências
RUN dotnet restore "src/Services/EstoqueService/EstoqueService.csproj"

# Copia todo o código fonte
COPY . .

# Builda a aplicação
WORKDIR "/src/src/Services/EstoqueService"
RUN dotnet build "EstoqueService.csproj" -c Release -o /app/build

# Publica a aplicação
FROM build AS publish
RUN dotnet publish "EstoqueService.csproj" -c Release -o /app/publish

# Estágio de runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Instala ferramentas necessárias para health checks e debugging
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Cria usuário não-root para segurança
RUN addgroup --system --gid 1001 appgroup \
    && adduser --system --uid 1001 --gid 1001 --shell /bin/false appuser

# Copia arquivos publicados
COPY --from=publish /app/publish .

# Define usuário para execução
USER appuser

# Expõe a porta da aplicação
EXPOSE 8080
EXPOSE 8081

# Define variáveis de ambiente
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check para monitoramento
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Comando de entrada
ENTRYPOINT ["dotnet", "EstoqueService.dll"]