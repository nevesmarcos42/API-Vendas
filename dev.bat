@echo off
REM Script de desenvolvimento para gerenciar os containers Docker no Windows
REM Fornece comandos uteis para desenvolvimento e debugging

setlocal enabledelayedexpansion

REM Funcao para mostrar ajuda
if "%1"=="help" goto :show_help
if "%1"=="" goto :show_help

REM Verificar se Docker esta instalado
docker --version >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Docker nao esta instalado!
    exit /b 1
)

docker-compose --version >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Docker Compose nao esta instalado!
    exit /b 1
)

REM Processar comandos
if "%1"=="build" goto :build_images
if "%1"=="up" goto :start_services
if "%1"=="down" goto :stop_services
if "%1"=="restart" goto :restart_services
if "%1"=="logs" goto :show_logs
if "%1"=="logs-gateway" goto :logs_gateway
if "%1"=="logs-vendas" goto :logs_vendas
if "%1"=="logs-estoque" goto :logs_estoque
if "%1"=="status" goto :show_status
if "%1"=="clean" goto :clean_all
if "%1"=="reset" goto :reset_all
if "%1"=="health" goto :check_health
if "%1"=="db-init" goto :init_databases

echo [ERROR] Comando desconhecido: %1
goto :show_help

:show_help
echo Script de desenvolvimento para Microservicos de Vendas
echo.
echo Uso: dev.bat [comando]
echo.
echo Comandos disponiveis:
echo   build         - Builda todas as imagens Docker
echo   up            - Inicia todos os servicos
echo   down          - Para todos os servicos
echo   restart       - Reinicia todos os servicos
echo   logs          - Mostra logs de todos os servicos
echo   logs-gateway  - Mostra logs do API Gateway
echo   logs-vendas   - Mostra logs do servico de vendas
echo   logs-estoque  - Mostra logs do servico de estoque
echo   status        - Mostra status dos containers
echo   clean         - Remove containers, volumes e imagens
echo   reset         - Limpa tudo e reconstroi
echo   health        - Verifica saude dos servicos
echo   db-init       - Inicializa bancos de dados
echo   help          - Mostra esta ajuda
exit /b 0

:build_images
echo [INFO] Buildando imagens Docker...
docker-compose build --no-cache
if %errorlevel% equ 0 (
    echo [SUCCESS] Imagens buildadas com sucesso!
) else (
    echo [ERROR] Falha ao buildar imagens!
    exit /b 1
)
exit /b 0

:start_services
echo [INFO] Iniciando servicos...
docker-compose up -d
if %errorlevel% equ 0 (
    echo [SUCCESS] Servicos iniciados!
    echo [INFO] Aguardando inicializacao dos servicos...
    timeout /t 30 /nobreak > nul
    goto :check_health
) else (
    echo [ERROR] Falha ao iniciar servicos!
    exit /b 1
)
exit /b 0

:stop_services
echo [INFO] Parando servicos...
docker-compose down
if %errorlevel% equ 0 (
    echo [SUCCESS] Servicos parados!
) else (
    echo [ERROR] Falha ao parar servicos!
    exit /b 1
)
exit /b 0

:restart_services
echo [INFO] Reiniciando servicos...
docker-compose restart
if %errorlevel% equ 0 (
    echo [SUCCESS] Servicos reiniciados!
) else (
    echo [ERROR] Falha ao reiniciar servicos!
    exit /b 1
)
exit /b 0

:show_logs
docker-compose logs -f
exit /b 0

:logs_gateway
docker-compose logs -f api-gateway
exit /b 0

:logs_vendas
docker-compose logs -f vendas-service
exit /b 0

:logs_estoque
docker-compose logs -f estoque-service
exit /b 0

:show_status
echo [INFO] Status dos containers:
docker-compose ps
exit /b 0

:clean_all
echo [WARNING] Esta operacao ira remover todos os containers, volumes e imagens!
set /p confirm="Tem certeza? (y/N): "
if /i "%confirm%"=="y" (
    echo [INFO] Parando containers...
    docker-compose down -v --remove-orphans
    
    echo [INFO] Removendo imagens...
    docker-compose down --rmi all
    
    echo [INFO] Limpando volumes nao utilizados...
    docker volume prune -f
    
    echo [SUCCESS] Limpeza concluida!
) else (
    echo [INFO] Operacao cancelada.
)
exit /b 0

:reset_all
echo [INFO] Fazendo reset completo do ambiente...
call :clean_all
call :build_images
call :start_services
echo [SUCCESS] Reset concluido!
exit /b 0

:check_health
echo [INFO] Verificando saude dos servicos...

curl -f -s "http://localhost:8000/health" >nul 2>&1
if %errorlevel% equ 0 (
    echo [SUCCESS] api-gateway esta saudavel
) else (
    echo [ERROR] api-gateway nao esta respondendo
)

curl -f -s "http://localhost:8001/health" >nul 2>&1
if %errorlevel% equ 0 (
    echo [SUCCESS] vendas-service esta saudavel
) else (
    echo [ERROR] vendas-service nao esta respondendo
)

curl -f -s "http://localhost:8002/health" >nul 2>&1
if %errorlevel% equ 0 (
    echo [SUCCESS] estoque-service esta saudavel
) else (
    echo [ERROR] estoque-service nao esta respondendo
)
exit /b 0

:init_databases
echo [INFO] Inicializando bancos de dados...
echo [INFO] Aguardando PostgreSQL...

:wait_postgres
docker-compose exec postgres pg_isready -U postgres >nul 2>&1
if %errorlevel% neq 0 (
    timeout /t 1 /nobreak > nul
    goto :wait_postgres
)

echo [SUCCESS] Bancos de dados prontos!
exit /b 0