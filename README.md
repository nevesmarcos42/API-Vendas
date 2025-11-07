# Sistema de Microserviços para Vendas

Este projeto implementa uma arquitetura de microserviços para gerenciamento de vendas e estoque, desenvolvido em .NET 8 com ASP.NET Core, Entity Framework Core, PostgreSQL e RabbitMQ.

## Visão Geral da Arquitetura

O sistema é composto por três componentes principais:

- **API Gateway**: Ponto de entrada centralizado com autenticação JWT e roteamento
- **Serviço de Vendas**: Gerencia operações de vendas e relatórios
- **Serviço de Estoque**: Controla produtos e movimentações de estoque
- **Biblioteca Compartilhada**: DTOs, interfaces e utilitários comuns

## Estrutura do Projeto

```
MicroservicesVendas/
├── src/
│   ├── Gateway/
│   │   └── ApiGateway/               # API Gateway principal
│   ├── Services/
│   │   ├── VendasService/            # Microserviço de vendas
│   │   └── EstoqueService/           # Microserviço de estoque
│   └── Shared/
│       └── SharedLibrary/            # Biblioteca compartilhada
└── README.md
```

## Tecnologias Utilizadas

- **.NET 8**: Framework principal
- **ASP.NET Core**: APIs REST
- **Entity Framework Core**: ORM para acesso a dados
- **PostgreSQL**: Banco de dados relacional
- **RabbitMQ**: Mensageria assíncrona
- **JWT (JSON Web Tokens)**: Autenticação e autorização
- **Swagger/OpenAPI**: Documentação das APIs

## Pré-requisitos

### Para Execução com Docker (Recomendado)

1. **Docker Desktop** ou **Docker Engine**
   - Windows: https://docs.docker.com/desktop/windows/
   - Mac: https://docs.docker.com/desktop/mac/
   - Linux: https://docs.docker.com/engine/install/

2. **Docker Compose** (incluído no Docker Desktop)
   ```bash
   # Verificar instalação
   docker --version
   docker-compose --version
   ```

### Para Desenvolvimento Local

1. **.NET 8 SDK**
   ```bash
   # Verificar instalação
   dotnet --version
   ```

2. **PostgreSQL 12+**
   - Download: https://www.postgresql.org/download/
   - Criar banco de dados: `VendasDB` e `EstoqueDB`
   - Usuário padrão: `postgres` / Senha: `postgres`

3. **RabbitMQ**
   - Download: https://www.rabbitmq.com/download.html
   - Porta padrão: 5672
   - Management UI: http://localhost:15672 (guest/guest)

4. **Git** (para clonar o projeto)

### Configuração do Ambiente

1. **PostgreSQL**
   ```sql
   -- Conectar como superusuário e criar bancos
   CREATE DATABASE "VendasDB";
   CREATE DATABASE "EstoqueDB";
   
   -- Verificar criação
   \l
   ```

2. **RabbitMQ**
   ```bash
   # Iniciar serviço (Windows)
   rabbitmq-server
   
   # Verificar status
   rabbitmqctl status
   ```

## Instalação e Configuração

### 1. Clonar o Repositório
```bash
git clone <url-do-repositorio>
cd API-Vendas
```

### 2. Executar com Docker (Método Simples)

```bash
# Construir e iniciar todos os serviços
docker-compose up -d

# Verificar se todos os serviços estão funcionando
docker-compose ps

# Verificar logs (opcional)
docker-compose logs -f
```

**Pronto!** Todos os serviços estarão disponíveis:
- API Gateway: http://localhost:8000
- Swagger UI: http://localhost:8000/swagger

### 3. Configuração Manual (Apenas para Desenvolvimento Local)

Se preferir executar sem Docker, siga estas configurações:

Edite os arquivos `appsettings.json` de cada serviço:

**Gateway (src/Gateway/ApiGateway/appsettings.json)**
```json
{
  "JwtSettings": {
    "SecretKey": "SuaChaveSecretaAqui",
    "Issuer": "MicroservicesVendas",
    "Audience": "MicroservicesVendas.Client"
  },
  "ServiceEndpoints": {
    "VendasServiceUrl": "https://localhost:7001",
    "EstoqueServiceUrl": "https://localhost:7002"
  }
}
```

**Serviço de Vendas (src/Services/VendasService/appsettings.json)**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=VendasDB;Username=postgres;Password=postgres",
    "RabbitMQ": "amqp://localhost"
  }
}
```

**Serviço de Estoque (src/Services/EstoqueService/appsettings.json)**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=EstoqueDB;Username=postgres;Password=postgres",
    "RabbitMQ": "amqp://localhost"
  }
}
```

### 4. Aplicar Migrações do Banco

```bash
# Serviço de Vendas
cd src/Services/VendasService
dotnet ef database update

# Serviço de Estoque
cd ../EstoqueService
dotnet ef database update
```

## Executando o Sistema

### Opção 1: Docker (Recomendado)

A maneira mais simples de executar todo o sistema é usando Docker. Todos os serviços, bancos de dados e dependências serão configurados automaticamente.

#### Pré-requisitos Docker
- **Docker Desktop** (Windows/Mac) ou **Docker Engine** (Linux)
- **Docker Compose** (geralmente incluído no Docker Desktop)

#### Executando com Docker

**Método Rápido:**
```bash
# Clone o repositório
git clone <url-do-repositorio>
cd API-Vendas

# Inicie todos os serviços
docker-compose up -d

# Verifique o status
docker-compose ps
```

**Usando Scripts de Desenvolvimento:**

**Linux/Mac:**
```bash
# Torne o script executável
chmod +x dev.sh

# Builde e inicie tudo
./dev.sh reset

# Ou execute comandos individuais
./dev.sh build    # Builda as imagens
./dev.sh up       # Inicia os serviços
./dev.sh health   # Verifica saúde dos serviços
./dev.sh logs     # Mostra logs
```

**Windows:**
```cmd
# Builde e inicie tudo
dev.bat reset

# Ou execute comandos individuais
dev.bat build    # Builda as imagens
dev.bat up       # Inicia os serviços
dev.bat health   # Verifica saúde dos serviços
dev.bat logs     # Mostra logs
```

#### Portas dos Serviços (Docker)
- **Nginx (Load Balancer)**: http://localhost
- **API Gateway**: http://localhost:8000
- **Serviço de Vendas**: http://localhost:8001
- **Serviço de Estoque**: http://localhost:8002
- **PostgreSQL**: localhost:5432
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)
- **Prometheus**: http://localhost:9090
- **Grafana**: http://localhost:3000 (admin/admin)

#### Comandos Docker Úteis

```bash
# Ver logs de um serviço específico
docker-compose logs -f vendas-service

# Parar todos os serviços
docker-compose down

# Rebuild uma imagem específica
docker-compose build vendas-service

# Limpar tudo (cuidado!)
docker-compose down -v --rmi all

# Verificar saúde dos containers
docker-compose ps
```

### Opção 2: Execução Local (Desenvolvimento)

Para desenvolvimento local sem Docker:

**Pré-requisitos:**
- .NET 8 SDK
- PostgreSQL 12+
- RabbitMQ

**Terminal 1 - API Gateway**
```bash
cd src/Gateway/ApiGateway
dotnet run --urls="https://localhost:7000"
```

**Terminal 2 - Serviço de Vendas**
```bash
cd src/Services/VendasService
dotnet run --urls="https://localhost:7001"
```

**Terminal 3 - Serviço de Estoque**
```bash
cd src/Services/EstoqueService
dotnet run --urls="https://localhost:7002"
```

### Opção 3: Visual Studio

1. Clique com botão direito na solução
2. Selecione "Configure Startup Projects"
3. Escolha "Multiple startup projects"
4. Configure todos os projetos como "Start"

## Utilizando as APIs

### 1. Autenticação

Primeiro, obtenha um token JWT:

```bash
curl -X POST "https://localhost:7000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "usuario": "admin",
    "senha": "admin123"
  }'
```

Usuários disponíveis:
- `admin` / `admin123` (Admin)
- `vendedor` / `vend123` (Vendedor)
- `estoquista` / `est123` (Estoquista)

### 2. Gerenciamento de Produtos

```bash
# Listar produtos
curl -X GET "https://localhost:7000/api/produtos" \
  -H "Authorization: Bearer SEU_TOKEN"

# Criar produto
curl -X POST "https://localhost:7000/api/produtos" \
  -H "Authorization: Bearer SEU_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "Notebook Dell",
    "descricao": "Notebook Dell Inspiron 15",
    "preco": 2499.99,
    "quantidadeEstoque": 10,
    "categoria": "Informática"
  }'
```

### 3. Operações de Venda

```bash
# Criar venda
curl -X POST "https://localhost:7000/api/vendas" \
  -H "Authorization: Bearer SEU_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "produtoId": 1,
    "quantidade": 2,
    "cliente": "João Silva"
  }'

# Listar vendas
curl -X GET "https://localhost:7000/api/vendas" \
  -H "Authorization: Bearer SEU_TOKEN"
```

### 4. Relatórios

```bash
# Relatório de vendas por período
curl -X GET "https://localhost:7000/api/vendas/relatorio?dataInicio=2024-01-01&dataFim=2024-12-31" \
  -H "Authorization: Bearer SEU_TOKEN"

# Produtos com estoque baixo
curl -X GET "https://localhost:7000/api/estoque/baixo" \
  -H "Authorization: Bearer SEU_TOKEN"
```

## Documentação das APIs

Após iniciar os serviços, acesse:

- **Gateway**: https://localhost:7000/swagger
- **Vendas**: https://localhost:7001/swagger
- **Estoque**: https://localhost:7002/swagger

## Comunicação Assíncrona

O sistema utiliza RabbitMQ para comunicação entre microserviços:

### Filas Principais
- `venda.registrada`: Notifica criação de vendas
- `estoque.alterado`: Notifica alterações no estoque
- `produto.criado`: Notifica criação de produtos

### Fluxo de Mensagens
1. **Venda criada** → Serviço de vendas publica evento
2. **Estoque atualizado** → Serviço de estoque consome evento e atualiza quantidade
3. **Notificação** → Outros serviços são notificados sobre mudanças

## Monitoramento

### Logs
Os serviços geram logs estruturados que incluem:
- Operações de banco de dados
- Comunicação entre serviços
- Eventos de mensageria
- Erros e exceções

### Métricas de Saúde
Endpoints de health check disponíveis:
- `GET /health` em cada serviço

## Desenvolvimento

### Estrutura de Desenvolvimento

1. **Modelos de Domínio**: Entidades que representam conceitos de negócio
2. **Repositórios**: Camada de acesso a dados
3. **Serviços**: Lógica de negócio e coordenação
4. **Controllers**: Endpoints REST
5. **DTOs**: Objetos de transferência de dados

### Padrões Utilizados

- **Repository Pattern**: Abstração do acesso a dados
- **Dependency Injection**: Inversão de controle
- **Event-Driven Architecture**: Comunicação via eventos
- **API Gateway Pattern**: Ponto de entrada centralizado
- **JWT Bearer Authentication**: Autenticação stateless

### Adicionando Novos Recursos

1. **Novo Endpoint**:
   - Criar DTO se necessário
   - Implementar lógica no serviço
   - Adicionar método no controller
   - Documentar no Swagger

2. **Nova Entidade**:
   - Criar modelo no namespace Models
   - Adicionar DbSet no contexto
   - Criar migração com `dotnet ef migrations add`
   - Aplicar com `dotnet ef database update`

3. **Nova Mensagem**:
   - Definir evento em SharedLibrary/Messages
   - Implementar publisher no serviço emissor
   - Implementar consumer no serviço receptor

## Troubleshooting

### Problemas Comuns

1. **Erro de Conexão com PostgreSQL**
   ```
   Solução: Verificar se o serviço está rodando e as credenciais estão corretas
   ```

2. **Erro de Conexão com RabbitMQ**
   ```
   Solução: Verificar se o RabbitMQ está rodando na porta 5672
   ```

3. **Token JWT Inválido**
   ```
   Solução: Verificar se a chave secreta está configurada corretamente em todos os serviços
   ```

4. **Erro 404 ao Chamar APIs**
   ```
   Solução: Verificar se os endpoints dos serviços estão corretos no Gateway
   ```

### Logs de Debug

Para habilitar logs detalhados, configure o nível de log no `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

## Desenvolvimento com Docker

### Estrutura dos Containers

O sistema é composto pelos seguintes containers:

- **microservices_postgres**: Banco PostgreSQL com databases VendasDB e EstoqueDB
- **microservices_rabbitmq**: Broker de mensagens RabbitMQ
- **microservices_estoque**: Microserviço de estoque
- **microservices_vendas**: Microserviço de vendas  
- **microservices_gateway**: API Gateway
- **microservices_nginx**: Load balancer (opcional)
- **microservices_prometheus**: Monitoramento (opcional)
- **microservices_grafana**: Dashboards (opcional)

### Comandos de Desenvolvimento

**Scripts Automatizados:**

```bash
# Linux/Mac
./dev.sh build        # Builda todas as imagens
./dev.sh up           # Inicia todos os serviços
./dev.sh down         # Para todos os serviços
./dev.sh restart      # Reinicia todos os serviços
./dev.sh logs         # Mostra todos os logs
./dev.sh logs-vendas  # Logs apenas do serviço de vendas
./dev.sh status       # Status dos containers
./dev.sh health       # Verifica saúde dos serviços
./dev.sh clean        # Remove tudo (cuidado!)
./dev.sh reset        # Reset completo

# Windows
dev.bat build         # Builda todas as imagens
dev.bat up            # Inicia todos os serviços
dev.bat down          # Para todos os serviços
dev.bat restart       # Reinicia todos os serviços
dev.bat logs          # Mostra todos os logs
dev.bat logs-vendas   # Logs apenas do serviço de vendas
dev.bat status        # Status dos containers
dev.bat health        # Verifica saúde dos serviços
dev.bat clean         # Remove tudo (cuidado!)
dev.bat reset         # Reset completo
```

**Comandos Docker Manuais:**

```bash
# Buildar apenas um serviço
docker-compose build vendas-service

# Iniciar apenas dependências (banco e RabbitMQ)
docker-compose up -d postgres rabbitmq

# Ver logs em tempo real
docker-compose logs -f

# Executar comando dentro de um container
docker-compose exec postgres psql -U postgres

# Parar e remover tudo
docker-compose down -v --remove-orphans

# Rebuild completo
docker-compose build --no-cache
```

### Volumes e Persistência

Os dados são persistidos em volumes Docker:

- **postgres_data**: Dados do PostgreSQL
- **rabbitmq_data**: Dados do RabbitMQ

Para backup dos dados:
```bash
# Backup do PostgreSQL
docker-compose exec postgres pg_dumpall -U postgres > backup.sql

# Restaurar backup
docker-compose exec -T postgres psql -U postgres < backup.sql
```

### Debugging

**Acessar logs detalhados:**
```bash
# Logs de um serviço específico
docker-compose logs -f vendas-service

# Logs com timestamps
docker-compose logs -f -t

# Últimas 100 linhas
docker-compose logs --tail=100 vendas-service
```

**Acessar shell do container:**
```bash
# PostgreSQL
docker-compose exec postgres bash

# RabbitMQ
docker-compose exec rabbitmq bash

# Serviço de vendas
docker-compose exec vendas-service bash
```

**Verificar conectividade:**
```bash
# Teste de conectividade entre containers
docker-compose exec vendas-service ping postgres
docker-compose exec api-gateway ping vendas-service
```

### Configuração de Desenvolvimento

Para desenvolvimento ativo, você pode:

1. **Hot Reload**: Monte o código fonte como volume (não recomendado para produção)
2. **Debug Remoto**: Configure debug remoto no Visual Studio/VS Code
3. **Logs Externos**: Redirecione logs para agregadores externos

### Monitoramento

Acesse os dashboards de monitoramento:

- **RabbitMQ Management**: http://localhost:15672
- **Prometheus**: http://localhost:9090  
- **Grafana**: http://localhost:3000

### Troubleshooting Docker

**Problemas Comuns:**

1. **Porta em uso:**
   ```bash
   # Verificar processos usando a porta
   netstat -tulpn | grep :8000
   
   # Parar containers conflitantes
   docker-compose down
   ```

2. **Falta de memória:**
   ```bash
   # Verificar uso de recursos
   docker stats
   
   # Limpar containers não utilizados
   docker system prune
   ```

3. **Build falha:**
   ```bash
   # Limpar cache e rebuild
   docker-compose build --no-cache
   
   # Verificar logs do build
   docker-compose build vendas-service
   ```

4. **Banco não conecta:**
   ```bash
   # Verificar se PostgreSQL está pronto
   docker-compose exec postgres pg_isready -U postgres
   
   # Verificar logs do banco
   docker-compose logs postgres
   ```

## Contribuição

1. Faça um fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## Próximos Passos

- [ ] Implementar cache distribuído (Redis)
- [ ] Adicionar métricas com Application Insights
- [ ] Implementar Circuit Breaker pattern
- [ ] Adicionar testes unitários e de integração
- [ ] Configurar CI/CD pipeline
- [ ] Dockerizar os serviços
- [ ] Implementar observabilidade com OpenTelemetry

## Suporte

Para dúvidas ou problemas:
1. Consulte a documentação do Swagger
2. Verifique os logs dos serviços
3. Abra uma issue no repositório

---

*Este projeto foi desenvolvido seguindo as melhores práticas de arquitetura de microserviços e clean code.*