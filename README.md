# API Vendas - Microserviços

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-512BD4?style=for-the-badge&logo=dotnet)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?style=for-the-badge&logo=postgresql)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-3.12-FF6600?style=for-the-badge&logo=rabbitmq)
![Docker](https://img.shields.io/badge/Docker-24.0-2496ED?style=for-the-badge&logo=docker)

Sistema de microserviços para gerenciamento completo de vendas e estoque com arquitetura distribuída, mensageria assíncrona e containerização Docker.

**Funcionalidades** • **Tecnologias** • **Instalação** • **Uso** • **API** • **Docker**

## Índice

- [Sobre o Projeto](#sobre-o-projeto)
- [Funcionalidades](#funcionalidades)
- [Tecnologias](#tecnologias)
- [Arquitetura](#arquitetura)
- [Instalação](#instalação)
- [Uso](#uso)
- [Documentação da API](#documentação-da-api)
- [Docker](#docker)
- [Monitoramento](#monitoramento)
- [Troubleshooting](#troubleshooting)
- [Contribuindo](#contribuindo)

## Sobre o Projeto

API Vendas é uma aplicação de microserviços robusta desenvolvida com .NET 8 que oferece um sistema completo de gerenciamento de vendas e controle de estoque. O projeto foi arquitetado com foco em escalabilidade, resiliência e boas práticas de desenvolvimento, utilizando padrões modernos de arquitetura distribuída.

### Principais Características

- **Arquitetura de Microserviços** - Serviços independentes e escaláveis
- **Autenticação JWT** - Sistema seguro de autenticação e autorização
- **API Gateway** - Ponto de entrada centralizado com roteamento inteligente
- **Mensageria Assíncrona** - Comunicação desacoplada via RabbitMQ
- **Containerização** - Deploy simplificado com Docker e Docker Compose
- **Documentação Swagger** - APIs autodocumentadas e testáveis
- **Controle de Estoque** - Gestão completa de produtos e movimentações
- **Sistema de Vendas** - CRUD completo com relatórios e validações
- **Load Balancer** - Nginx para distribuição de carga
- **Monitoramento** - Métricas com Prometheus e dashboards Grafana

## Funcionalidades

### Backend - Microserviços

#### API Gateway

- Ponto de entrada único para todos os serviços
- Autenticação JWT centralizada
- Roteamento inteligente de requisições
- Middleware de proxy para serviços downstream
- Controle de acesso baseado em roles

#### Serviço de Estoque

- CRUD completo de produtos
- Controle de quantidade em estoque
- Movimentações de entrada e saída
- Validação de disponibilidade
- Notificações de estoque baixo
- Sincronização via eventos

#### Serviço de Vendas

- Registro de vendas
- Validação de estoque disponível
- Cálculo automático de totais
- Relatórios por período
- Histórico de transações
- Integração com serviço de estoque

#### Comunicação Assíncrona

- Eventos de venda registrada
- Eventos de alteração de estoque
- Eventos de criação de produtos
- Processamento em background
- Garantia de entrega de mensagens

## Tecnologias

### Backend

| Tecnologia            | Versão | Descrição                 |
| --------------------- | ------ | ------------------------- |
| .NET                  | 8.0    | Framework principal       |
| ASP.NET Core          | 8.0    | Framework web             |
| Entity Framework Core | 8.0    | ORM para acesso a dados   |
| PostgreSQL            | 16+    | Banco de dados relacional |
| RabbitMQ              | 3.12+  | Message broker            |
| JWT                   | -      | Autenticação stateless    |
| Swagger/OpenAPI       | 6.4+   | Documentação de APIs      |

### DevOps

- **Docker** - Containerização de aplicações
- **Docker Compose** - Orquestração de containers
- **Nginx** - Load balancer e reverse proxy
- **Prometheus** - Coleta de métricas
- **Grafana** - Visualização de métricas

## Arquitetura

### Estrutura de Microserviços

```
MicroservicesVendas/
├── src/
│   ├── Gateway/
│   │   └── ApiGateway/               # API Gateway principal
│   │       ├── Controllers/          # Endpoints REST
│   │       ├── Middleware/           # Proxy middleware
│   │       └── Configuration/        # Configurações JWT
│   ├── Services/
│   │   ├── VendasService/            # Microserviço de vendas
│   │   │   ├── Controllers/          # Endpoints de vendas
│   │   │   ├── Data/                 # Contexto EF Core
│   │   │   ├── Models/               # Entidades de venda
│   │   │   ├── Repositories/         # Acesso a dados
│   │   │   └── Services/             # Lógica de negócio
│   │   └── EstoqueService/           # Microserviço de estoque
│   │       ├── Controllers/          # Endpoints de produtos
│   │       ├── Data/                 # Contexto EF Core
│   │       ├── Models/               # Entidades de produto
│   │       ├── Repositories/         # Acesso a dados
│   │       └── Services/             # Lógica de negócio
│   └── Shared/
│       └── SharedLibrary/            # Biblioteca compartilhada
│           ├── Constants/            # Constantes da aplicação
│           ├── Interfaces/           # Interfaces comuns
│           ├── Messages/             # DTOs de mensagens
│           └── MessageBus/           # Cliente RabbitMQ
└── docker/
    ├── docker-compose.yml            # Orquestração
    ├── init-databases.sql            # Scripts SQL
    ├── nginx.conf                    # Config Nginx
    └── prometheus.yml                # Config Prometheus
```

### Banco de Dados - Modelo de Dados

```
┌─────────────────┐       ┌─────────────────┐
│     Venda       │       │    Produto      │
├─────────────────┤       ├─────────────────┤
│ Id              │       │ Id              │
│ ProdutoId       │───────│ Nome            │
│ Quantidade      │       │ Descricao       │
│ Cliente         │       │ Preco           │
│ DataVenda       │       │ Quantidade      │
│ ValorTotal      │       │ Categoria       │
└─────────────────┘       └─────────────────┘
```

### Comunicação Entre Serviços

```
┌──────────────┐      ┌──────────────┐      ┌──────────────┐
│              │      │              │      │              │
│   Cliente    │─────▶│  API Gateway │─────▶│   Serviços   │
│              │      │              │      │              │
└──────────────┘      └──────────────┘      └──────────────┘
                             │
                             ├─────▶ VendasService
                             ├─────▶ EstoqueService
                             └─────▶ Autenticação JWT

                      ┌──────────────┐
                      │   RabbitMQ   │
                      │  (Mensagens) │
                      └──────────────┘
                             ▲
                             │
                ┌────────────┴────────────┐
                │                         │
         VendasService              EstoqueService
```

## Instalação

### Pré-requisitos

#### Para Execução com Docker (Recomendado)

- **Docker Desktop** ou **Docker Engine**
  - [Windows](https://docs.docker.com/desktop/windows/)
  - [Mac](https://docs.docker.com/desktop/mac/)
  - [Linux](https://docs.docker.com/engine/install/)
- **Docker Compose** (incluído no Docker Desktop)

```bash
# Verificar instalação
docker --version
docker-compose --version
```

#### Para Desenvolvimento Local

1. **.NET 8 SDK**

   ```bash
   # Verificar instalação
   dotnet --version
   ```

2. **PostgreSQL 12+**

   - [Download PostgreSQL](https://www.postgresql.org/download/)
   - Criar bancos: `VendasDB` e `EstoqueDB`
   - Usuário: `postgres` / Senha: `postgres`

3. **RabbitMQ**

   - [Download RabbitMQ](https://www.rabbitmq.com/download.html)
   - Porta padrão: `5672`
   - Management UI: `http://localhost:15672` (guest/guest)

4. **Git** (para clonar o projeto)

### Instalação com Docker (Recomendado)

#### 1. Clone o repositório

```bash
git clone https://github.com/nevesmarcos42/API-Vendas.git
cd API-Vendas
```

#### 2. Inicie a aplicação

```bash
docker-compose up -d
```

Pronto! A aplicação estará rodando em:

- **Frontend/Nginx**: `http://localhost`
- **API Gateway**: `http://localhost:8000`
- **Swagger UI Gateway**: `http://localhost:8000/swagger`
- **Serviço Vendas**: `http://localhost:8001`
- **Serviço Estoque**: `http://localhost:8002`
- **PostgreSQL**: `localhost:5432`
- **RabbitMQ Management**: `http://localhost:15672`

#### 3. Verificar status dos containers

```bash
docker-compose ps
```

#### 4. Parar a aplicação

```bash
docker-compose down
```

### Instalação Manual (Desenvolvimento Local)

#### 1. Configure os bancos de dados

```sql
-- Conectar ao PostgreSQL como superusuário
CREATE DATABASE "VendasDB";
CREATE DATABASE "EstoqueDB";
```

#### 2. Configure as connection strings

Edite os arquivos `appsettings.json`:

**VendasService:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=VendasDB;Username=postgres;Password=postgres",
    "RabbitMQ": "amqp://localhost"
  }
}
```

**EstoqueService:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=EstoqueDB;Username=postgres;Password=postgres",
    "RabbitMQ": "amqp://localhost"
  }
}
```

#### 3. Aplique as migrações

```bash
# Serviço de Vendas
cd src/Services/VendasService
dotnet ef database update

# Serviço de Estoque
cd ../EstoqueService
dotnet ef database update
```

## Uso

### Primeiro Acesso

1. **Inicie a aplicação**:

   - Com Docker: `docker-compose up -d`
   - Manual: Inicie cada serviço em terminais separados

2. **Acesse a documentação Swagger**:

   - Gateway: `http://localhost:8000/swagger`
   - Vendas: `http://localhost:8001/swagger`
   - Estoque: `http://localhost:8002/swagger`

3. **Obtenha um token JWT**:
   - Use o endpoint `/api/auth/login`
   - Usuários disponíveis:
     - Admin: `admin` / `admin123`
     - Vendedor: `vendedor` / `vend123`
     - Estoquista: `estoquista` / `est123`

### Funcionalidades Principais

#### Autenticar

```bash
# Login
POST /api/auth/login
{
  "usuario": "admin",
  "senha": "admin123"
}
```

#### Gerenciar Produtos

```bash
# Listar produtos
GET /api/produtos

# Criar produto (requer ADMIN)
POST /api/produtos
{
  "nome": "Notebook Dell",
  "descricao": "i7, 16GB RAM",
  "preco": 4500.00,
  "quantidadeEstoque": 10,
  "categoria": "Informática"
}

# Atualizar produto
PUT /api/produtos/{id}

# Deletar produto
DELETE /api/produtos/{id}
```

#### Registrar Vendas

```bash
# Criar venda
POST /api/vendas
{
  "produtoId": 1,
  "quantidade": 2,
  "cliente": "João Silva"
}

# Listar vendas
GET /api/vendas

# Buscar venda específica
GET /api/vendas/{id}
```

#### Visualizar Relatórios

```bash
# Relatório de vendas por período
GET /api/vendas/relatorio?dataInicio=2024-01-01&dataFim=2024-12-31

# Produtos com estoque baixo
GET /api/estoque/baixo
```

## Documentação da API

A documentação interativa está disponível via Swagger UI após iniciar os serviços.

### Endpoints Principais

#### API Gateway (`http://localhost:8000`)

| Método | Endpoint          | Descrição          | Auth  |
| ------ | ----------------- | ------------------ | ----- |
| POST   | `/api/auth/login` | Autenticar usuário | Não   |
| GET    | `/api/produtos`   | Listar produtos    | Sim   |
| POST   | `/api/produtos`   | Criar produto      | Admin |
| GET    | `/api/vendas`     | Listar vendas      | Sim   |
| POST   | `/api/vendas`     | Criar venda        | Sim   |

### Exemplo de Requisição

#### Registrar Usuário e Autenticar

```bash
# Login
curl -X POST "http://localhost:8000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "usuario": "admin",
    "senha": "admin123"
  }'
```

#### Criar Produto (com token)

```bash
curl -X POST "http://localhost:8000/api/produtos" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer SEU_TOKEN_JWT" \
  -d '{
    "nome": "Mouse Logitech",
    "descricao": "Mouse sem fio",
    "preco": 150.00,
    "quantidadeEstoque": 50,
    "categoria": "Periféricos"
  }'
```

#### Criar Venda

```bash
curl -X POST "http://localhost:8000/api/vendas" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer SEU_TOKEN_JWT" \
  -d '{
    "produtoId": 1,
    "quantidade": 3,
    "cliente": "Maria Santos"
  }'
```

### Autenticação

Todas as rotas protegidas requerem um token JWT no header:

```
Authorization: Bearer {seu_token_jwt}
```

## Docker

### Arquitetura de Containers

A aplicação é composta por containers Docker orquestrados via Docker Compose:

#### Containers

1. **microservices_postgres** - PostgreSQL 16
2. **microservices_rabbitmq** - RabbitMQ 3.12
3. **microservices_estoque** - Serviço de Estoque
4. **microservices_vendas** - Serviço de Vendas
5. **microservices_gateway** - API Gateway
6. **microservices_nginx** - Load Balancer
7. **microservices_prometheus** - Métricas
8. **microservices_grafana** - Dashboards

#### Volumes

- `postgres_data` - Persistência do banco de dados
- `rabbitmq_data` - Persistência das filas

#### Network

- `microservices_network` - Comunicação entre containers

### Scripts de Desenvolvimento

O projeto inclui scripts para facilitar o desenvolvimento:

#### Linux/Mac (`dev.sh`)

```bash
# Torne o script executável
chmod +x dev.sh

# Comandos disponíveis
./dev.sh build        # Build das imagens
./dev.sh up           # Iniciar serviços
./dev.sh down         # Parar serviços
./dev.sh restart      # Reiniciar serviços
./dev.sh logs         # Ver logs
./dev.sh logs-vendas  # Logs do serviço de vendas
./dev.sh status       # Status dos containers
./dev.sh health       # Verificar saúde
./dev.sh clean        # Limpar tudo
./dev.sh reset        # Reset completo
```

#### Windows (`dev.bat`)

```cmd
# Comandos disponíveis
dev.bat build         # Build das imagens
dev.bat up            # Iniciar serviços
dev.bat down          # Parar serviços
dev.bat restart       # Reiniciar serviços
dev.bat logs          # Ver logs
dev.bat logs-vendas   # Logs do serviço de vendas
dev.bat status        # Status dos containers
dev.bat health        # Verificar saúde
dev.bat clean         # Limpar tudo
dev.bat reset         # Reset completo
```

### Comandos Docker Úteis

```bash
# Ver logs de um serviço específico
docker-compose logs -f vendas-service

# Executar comando em um container
docker-compose exec postgres psql -U postgres

# Rebuild de um serviço específico
docker-compose build vendas-service

# Parar e remover tudo
docker-compose down -v

# Limpar cache e rebuild
docker-compose build --no-cache
```

## Monitoramento

### RabbitMQ Management

Acesse o painel de gerenciamento do RabbitMQ:

- **URL**: `http://localhost:15672`
- **Usuário**: `guest`
- **Senha**: `guest`

Funcionalidades:

- Visualizar filas e mensagens
- Monitorar conexões
- Gerenciar exchanges
- Ver estatísticas de consumo

### Prometheus

Coleta de métricas da aplicação:

- **URL**: `http://localhost:9090`
- Queries PromQL
- Alertas configuráveis
- Histórico de métricas

### Grafana

Dashboards visuais de métricas:

- **URL**: `http://localhost:3000`
- **Usuário**: `admin`
- **Senha**: `admin`

Dashboards disponíveis:

- Saúde dos serviços
- Performance de APIs
- Uso de recursos
- Métricas de negócio

### Health Checks

Cada serviço expõe um endpoint de saúde:

```bash
# Gateway
GET http://localhost:8000/health

# Serviço de Vendas
GET http://localhost:8001/health

# Serviço de Estoque
GET http://localhost:8002/health
```

## Troubleshooting

### Problemas Comuns

#### Erro de Conexão com PostgreSQL

```
Solução: Verificar se o PostgreSQL está rodando
docker-compose ps postgres
docker-compose logs postgres
```

#### Erro de Conexão com RabbitMQ

```
Solução: Verificar se o RabbitMQ está rodando
docker-compose ps rabbitmq
docker-compose logs rabbitmq
```

#### Token JWT Inválido

```
Solução: Verificar configuração da chave secreta
- Conferir appsettings.json de todos os serviços
- Garantir que a mesma chave está em todos
```

#### Porta em Uso

```bash
# Windows
netstat -ano | findstr :8000
taskkill /PID <pid> /F

# Linux/Mac
lsof -i :8000
kill -9 <pid>
```

#### Containers Não Iniciam

```bash
# Ver logs detalhados
docker-compose logs

# Rebuild completo
docker-compose down -v
docker-compose build --no-cache
docker-compose up -d
```

### Logs de Debug

Habilitar logs detalhados no `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Information",
      "Microsoft.AspNetCore": "Debug"
    }
  }
}
```

### Backup e Restauração

#### Backup do PostgreSQL

```bash
# Backup completo
docker-compose exec postgres pg_dumpall -U postgres > backup.sql

# Backup de um banco específico
docker-compose exec postgres pg_dump -U postgres VendasDB > vendas_backup.sql
```

#### Restaurar Backup

```bash
# Restaurar completo
docker-compose exec -T postgres psql -U postgres < backup.sql

# Restaurar banco específico
docker-compose exec -T postgres psql -U postgres VendasDB < vendas_backup.sql
```

## Regras de Negócio

### Produtos

- Nome deve ser único
- Preço deve ser maior que zero
- Quantidade em estoque não pode ser negativa
- Categoria é obrigatória

### Vendas

- Produto deve existir e ter estoque disponível
- Quantidade deve ser maior que zero
- Cliente é obrigatório
- Valor total é calculado automaticamente
- Estoque é decrementado automaticamente

### Autenticação

- Senha mínima: 6 caracteres
- Token JWT expira em 24 horas
- Roles disponíveis: USER, ADMIN, VENDEDOR, ESTOQUISTA
- Apenas ADMIN pode criar/editar produtos

## Contribuindo

Contribuições são bem-vindas! Siga os passos:

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/MinhaFeature`)
3. Commit suas mudanças (`git commit -m 'Adiciona MinhaFeature'`)
4. Push para a branch (`git push origin feature/MinhaFeature`)
5. Abra um Pull Request

### Padrões de Código

#### Backend

- Seguir convenções do C# e .NET
- Usar async/await para operações I/O
- Implementar tratamento de erros adequado
- Documentar métodos públicos
- Escrever testes para novas funcionalidades

#### Arquitetura

- Manter separação de responsabilidades
- Usar Repository Pattern
- Implementar Dependency Injection
- Seguir princípios SOLID
- Event-Driven Architecture para comunicação assíncrona

## Próximos Passos

- [ ] Implementar cache distribuído com Redis
- [ ] Adicionar testes unitários e de integração
- [ ] Implementar Circuit Breaker pattern
- [ ] Adicionar autenticação OAuth2
- [ ] Configurar CI/CD pipeline
- [ ] Implementar observabilidade com OpenTelemetry
- [ ] Adicionar rate limiting
- [ ] Implementar saga pattern para transações distribuídas

## Licença

Este projeto foi desenvolvido como projeto de estudo de arquitetura de microserviços com .NET.

**Desenvolvido com .NET 8 e ASP.NET Core**

**Versão**: 1.0.0

**Última Atualização**: Novembro 2025

---

_Desenvolvido seguindo as melhores práticas de arquitetura de microserviços e clean code._
