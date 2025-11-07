#!/bin/bash

# Script de desenvolvimento para gerenciar os containers Docker
# Fornece comandos úteis para desenvolvimento e debugging

set -e

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Funções de utility
print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Função para mostrar ajuda
show_help() {
    echo "Script de desenvolvimento para Microserviços de Vendas"
    echo ""
    echo "Uso: ./dev.sh [comando]"
    echo ""
    echo "Comandos disponíveis:"
    echo "  build         - Builda todas as imagens Docker"
    echo "  up            - Inicia todos os serviços"
    echo "  down          - Para todos os serviços"
    echo "  restart       - Reinicia todos os serviços"
    echo "  logs          - Mostra logs de todos os serviços"
    echo "  logs-gateway  - Mostra logs do API Gateway"
    echo "  logs-vendas   - Mostra logs do serviço de vendas"
    echo "  logs-estoque  - Mostra logs do serviço de estoque"
    echo "  status        - Mostra status dos containers"
    echo "  clean         - Remove containers, volumes e imagens"
    echo "  reset         - Limpa tudo e reconstroi"
    echo "  health        - Verifica saúde dos serviços"
    echo "  db-init       - Inicializa bancos de dados"
    echo "  help          - Mostra esta ajuda"
}

# Função para verificar se Docker está instalado
check_docker() {
    if ! command -v docker &> /dev/null; then
        print_error "Docker não está instalado!"
        exit 1
    fi
    
    if ! command -v docker-compose &> /dev/null; then
        print_error "Docker Compose não está instalado!"
        exit 1
    fi
}

# Função para buildar imagens
build_images() {
    print_info "Buildando imagens Docker..."
    docker-compose build --no-cache
    print_success "Imagens buildadas com sucesso!"
}

# Função para iniciar serviços
start_services() {
    print_info "Iniciando serviços..."
    docker-compose up -d
    print_success "Serviços iniciados!"
    
    print_info "Aguardando inicialização dos serviços..."
    sleep 30
    
    check_health
}

# Função para parar serviços
stop_services() {
    print_info "Parando serviços..."
    docker-compose down
    print_success "Serviços parados!"
}

# Função para reiniciar serviços
restart_services() {
    print_info "Reiniciando serviços..."
    docker-compose restart
    print_success "Serviços reiniciados!"
}

# Função para mostrar logs
show_logs() {
    local service=$1
    if [ -z "$service" ]; then
        docker-compose logs -f
    else
        docker-compose logs -f $service
    fi
}

# Função para mostrar status
show_status() {
    print_info "Status dos containers:"
    docker-compose ps
}

# Função para limpeza completa
clean_all() {
    print_warning "Esta operação irá remover todos os containers, volumes e imagens!"
    read -p "Tem certeza? (y/N): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        print_info "Parando containers..."
        docker-compose down -v --remove-orphans
        
        print_info "Removendo imagens..."
        docker-compose down --rmi all
        
        print_info "Limpando volumes não utilizados..."
        docker volume prune -f
        
        print_success "Limpeza concluída!"
    else
        print_info "Operação cancelada."
    fi
}

# Função para reset completo
reset_all() {
    print_info "Fazendo reset completo do ambiente..."
    clean_all
    build_images
    start_services
    print_success "Reset concluído!"
}

# Função para verificar saúde dos serviços
check_health() {
    print_info "Verificando saúde dos serviços..."
    
    services=("api-gateway:8000" "vendas-service:8001" "estoque-service:8002")
    
    for service in "${services[@]}"; do
        IFS=':' read -ra ADDR <<< "$service"
        service_name=${ADDR[0]}
        port=${ADDR[1]}
        
        if curl -f -s "http://localhost:$port/health" > /dev/null; then
            print_success "$service_name está saudável"
        else
            print_error "$service_name não está respondendo"
        fi
    done
}

# Função para inicializar bancos de dados
init_databases() {
    print_info "Inicializando bancos de dados..."
    
    # Aguarda PostgreSQL estar pronto
    print_info "Aguardando PostgreSQL..."
    until docker-compose exec postgres pg_isready -U postgres; do
        sleep 1
    done
    
    print_success "Bancos de dados prontos!"
}

# Comando principal
case "${1:-help}" in
    build)
        check_docker
        build_images
        ;;
    up)
        check_docker
        start_services
        ;;
    down)
        check_docker
        stop_services
        ;;
    restart)
        check_docker
        restart_services
        ;;
    logs)
        check_docker
        show_logs
        ;;
    logs-gateway)
        check_docker
        show_logs "api-gateway"
        ;;
    logs-vendas)
        check_docker
        show_logs "vendas-service"
        ;;
    logs-estoque)
        check_docker
        show_logs "estoque-service"
        ;;
    status)
        check_docker
        show_status
        ;;
    clean)
        check_docker
        clean_all
        ;;
    reset)
        check_docker
        reset_all
        ;;
    health)
        check_docker
        check_health
        ;;
    db-init)
        check_docker
        init_databases
        ;;
    help)
        show_help
        ;;
    *)
        print_error "Comando desconhecido: $1"
        show_help
        exit 1
        ;;
esac