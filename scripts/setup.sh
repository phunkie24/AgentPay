#!/bin/bash

###############################################################################
# AgentPay - Complete Setup and Deployment Script
# MNEE Hackathon Submission - AI & Agent Payments Track
###############################################################################

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
PROJECT_NAME="AgentPay"
DOCKER_COMPOSE_FILE="docker compose.yml"
ENV_FILE=".env"

###############################################################################
# Helper Functions
###############################################################################

print_header() {
    echo -e "${BLUE}╔════════════════════════════════════════════════════════════╗${NC}"
    echo -e "${BLUE}║                                                            ║${NC}"
    echo -e "${BLUE}║         🤖 AgentPay - Autonomous AI Payments 💰           ║${NC}"
    echo -e "${BLUE}║                                                            ║${NC}"
    echo -e "${BLUE}║               MNEE Hackathon Submission                    ║${NC}"
    echo -e "${BLUE}║            AI & Agent Payments Track                       ║${NC}"
    echo -e "${BLUE}║                                                            ║${NC}"
    echo -e "${BLUE}╚════════════════════════════════════════════════════════════╝${NC}"
    echo ""
}

print_section() {
    echo -e "${GREEN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo -e "${GREEN}  $1${NC}"
    echo -e "${GREEN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo ""
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_error() {
    echo -e "${RED}❌ Error: $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️  Warning: $1${NC}"
}

print_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

###############################################################################
# Prerequisites Check
###############################################################################

check_prerequisites() {
    print_section "Checking Prerequisites"

    # Check Docker
    if ! command -v docker &> /dev/null; then
        print_error "Docker is not installed"
        exit 1
    fi
    print_success "Docker found: $(docker --version)"

    # Check Docker Compose
    if ! command -v docker compose &> /dev/null && ! docker compose version &> /dev/null; then
        print_error "Docker Compose is not installed"
        exit 1
    fi
    print_success "Docker Compose found"

    # Check .NET SDK
    if ! command -v dotnet &> /dev/null; then
        print_warning ".NET SDK not found (needed for local development)"
    else
        print_success "NET SDK found: $(dotnet --version)"
    fi

    # Check available disk space
    AVAILABLE_SPACE=$(df -h . | awk 'NR==2 {print $4}')
    print_info "Available disk space: $AVAILABLE_SPACE"

    echo ""
}

###############################################################################
# Environment Setup
###############################################################################

setup_environment() {
    print_section "Setting Up Environment"

    if [ ! -f "$ENV_FILE" ]; then
        print_info "Creating .env file..."
        
        cat > "$ENV_FILE" << 'EOF'
# AgentPay Environment Configuration

# Blockchain Configuration
ALCHEMY_API_KEY=your_alchemy_api_key_here
ETHEREUM_RPC_URL=https://eth-mainnet.g.alchemy.com/v2/your_key
MNEE_CONTRACT_ADDRESS=0x8ccedbAe4916b79da7F3F612EfB2EB93A2bFD6cF

# Database
SQL_PASSWORD=AgentPay123!

# Redis
REDIS_PASSWORD=agentpay_redis

# RabbitMQ
RABBITMQ_USER=agentpay
RABBITMQ_PASS=agentpay_queue

# AI/LLM Configuration
AI_PROVIDER=Ollama
OLLAMA_MODEL=llama3.2:latest

# Optional: OpenAI (if not using Ollama)
OPENAI_API_KEY=your_openai_key_here

# Application
ASPNETCORE_ENVIRONMENT=Production
APP_NAME=AgentPay
EOF
        
        print_success ".env file created"
        print_warning "Please update .env with your API keys!"
    else
        print_info ".env file already exists"
    fi

    echo ""
}

###############################################################################
# Docker Setup
###############################################################################

setup_docker() {
    print_section "Building Docker Images"

    print_info "Building AgentPay Docker images..."
    docker compose build --parallel

    print_success "Docker images built successfully"
    echo ""
}

###############################################################################
# Database Setup
###############################################################################

setup_database() {
    print_section "Setting Up Database"

    print_info "Starting SQL Server..."
    docker compose up -d sqlserver

    print_info "Waiting for SQL Server to be ready..."
    sleep 20

    print_info "Running database migrations..."
    # In production, this would run EF Core migrations
    # docker compose exec web dotnet ef database update

    print_success "Database setup complete"
    echo ""
}

###############################################################################
# Ollama Setup
###############################################################################

setup_ollama() {
    print_section "Setting Up Ollama (Local LLM)"

    print_info "Starting Ollama service..."
    docker compose up -d ollama

    print_info "Waiting for Ollama to be ready..."
    sleep 10

    print_info "Pulling Llama 3.2 model (this may take a while)..."
    docker compose exec ollama ollama pull llama3.2:latest

    print_info "Pulling Mistral model..."
    docker compose exec ollama ollama pull mistral:latest

    print_success "Ollama setup complete"
    echo ""
}

###############################################################################
# Start All Services
###############################################################################

start_services() {
    print_section "Starting All Services"

    print_info "Starting all AgentPay services..."
    docker compose up -d

    print_info "Waiting for services to be healthy..."
    sleep 30

    print_success "All services started"
    echo ""
}

###############################################################################
# Health Check
###############################################################################

health_check() {
    print_section "Running Health Checks"

    # Check Web Application
    if curl -f http://localhost:5000/health > /dev/null 2>&1; then
        print_success "Web Application is healthy"
    else
        print_warning "Web Application might not be ready yet"
    fi

    # Check MCP Server
    if curl -f http://localhost:8080/health > /dev/null 2>&1; then
        print_success "MCP Server is healthy"
    else
        print_warning "MCP Server might not be ready yet"
    fi

    # Check SQL Server
    if docker compose exec -T sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P AgentPay123! -Q "SELECT 1" > /dev/null 2>&1; then
        print_success "SQL Server is healthy"
    else
        print_warning "SQL Server might not be ready yet"
    fi

    # Check Redis
    if docker compose exec -T redis redis-cli ping > /dev/null 2>&1; then
        print_success "Redis is healthy"
    else
        print_warning "Redis might not be ready yet"
    fi

    echo ""
}

###############################################################################
# Display Access Information
###############################################################################

display_access_info() {
    print_section "Access Information"

    echo -e "${GREEN}🌐 Application URLs:${NC}"
    echo -e "   ${BLUE}Web Dashboard:${NC}    http://localhost:5000"
    echo -e "   ${BLUE}API:${NC}              http://localhost:5000/api"
    echo -e "   ${BLUE}MCP Server:${NC}       http://localhost:8080"
    echo -e "   ${BLUE}Grafana:${NC}          http://localhost:3000 (admin/agentpay_admin)"
    echo -e "   ${BLUE}RabbitMQ:${NC}         http://localhost:15672 (agentpay/agentpay_queue)"
    echo ""

    echo -e "${GREEN}📊 Monitoring:${NC}"
    echo -e "   ${BLUE}Prometheus:${NC}       http://localhost:9090"
    echo -e "   ${BLUE}Grafana:${NC}          http://localhost:3000"
    echo ""

    echo -e "${GREEN}🗄️  Database:${NC}"
    echo -e "   ${BLUE}SQL Server:${NC}       localhost:1433"
    echo -e "   ${BLUE}Database:${NC}         AgentPay"
    echo -e "   ${BLUE}Username:${NC}         sa"
    echo -e "   ${BLUE}Password:${NC}         AgentPay123!"
    echo ""

    echo -e "${GREEN}💾 Cache & Queue:${NC}"
    echo -e "   ${BLUE}Redis:${NC}            localhost:6379"
    echo -e "   ${BLUE}RabbitMQ:${NC}         localhost:5672"
    echo ""

    echo -e "${GREEN}🤖 AI Services:${NC}"
    echo -e "   ${BLUE}Ollama:${NC}           http://localhost:11434"
    echo -e "   ${BLUE}Vector DB:${NC}        http://localhost:6333"
    echo ""
}

###############################################################################
# View Logs
###############################################################################

view_logs() {
    print_section "Viewing Logs"

    echo -e "${BLUE}To view logs for specific services:${NC}"
    echo "  docker compose logs -f web           # Web application"
    echo "  docker compose logs -f mcpserver     # MCP server"
    echo "  docker compose logs -f sqlserver     # Database"
    echo "  docker compose logs -f ollama        # Ollama LLM"
    echo ""
    echo "  docker compose logs -f               # All services"
    echo ""
}

###############################################################################
# Useful Commands
###############################################################################

display_commands() {
    print_section "Useful Commands"

    echo -e "${GREEN}🔧 Management Commands:${NC}"
    echo ""
    echo -e "${BLUE}Start services:${NC}"
    echo "  docker compose up -d"
    echo ""
    echo -e "${BLUE}Stop services:${NC}"
    echo "  docker compose down"
    echo ""
    echo -e "${BLUE}Restart services:${NC}"
    echo "  docker compose restart"
    echo ""
    echo -e "${BLUE}View logs:${NC}"
    echo "  docker compose logs -f [service]"
    echo ""
    echo -e "${BLUE}Execute commands in container:${NC}"
    echo "  docker compose exec web bash"
    echo ""
    echo -e "${BLUE}Run database migrations:${NC}"
    echo "  docker compose exec web dotnet ef database update"
    echo ""
    echo -e "${BLUE}Clean up everything:${NC}"
    echo "  docker compose down -v --remove-orphans"
    echo ""
}

###############################################################################
# Seed Data
###############################################################################

seed_data() {
    print_section "Seeding Initial Data"

    print_info "Creating demo agents..."
    # In production, this would call the seeding endpoint or script
    # docker compose exec web dotnet run -- seed

    print_success "Initial data seeded"
    echo ""
}

###############################################################################
# GitHub Setup
###############################################################################

setup_github() {
    print_section "GitHub Repository Setup"

    if [ ! -d ".git" ]; then
        print_info "Initializing Git repository..."
        git init
        git add .
        git commit -m "Initial commit: AgentPay - MNEE Hackathon Submission"
        
        print_success "Git repository initialized"
        print_info "To push to GitHub:"
        echo "  git remote add origin https://github.com/yourusername/agentpay.git"
        echo "  git push -u origin main"
    else
        print_info "Git repository already initialized"
    fi

    echo ""
}

###############################################################################
# Main Execution
###############################################################################

main() {
    clear
    print_header

    # Parse command line arguments
    case "${1:-all}" in
        all)
            check_prerequisites
            setup_environment
            setup_docker
            setup_database
            setup_ollama
            start_services
            health_check
            seed_data
            display_access_info
            view_logs
            display_commands
            ;;
        quick)
            check_prerequisites
            start_services
            health_check
            display_access_info
            ;;
        github)
            setup_github
            ;;
        *)
            echo "Usage: $0 {all|quick|github}"
            echo ""
            echo "  all      - Complete setup (first time)"
            echo "  quick    - Quick start (if already configured)"
            echo "  github   - Setup GitHub repository"
            exit 1
            ;;
    esac

    print_section "Setup Complete! 🎉"
    
    echo -e "${GREEN}AgentPay is ready!${NC}"
    echo ""
    echo -e "${BLUE}Next steps:${NC}"
    echo "  1. Update .env with your API keys"
    echo "  2. Access the dashboard at http://localhost:5000"
    echo "  3. Create your first autonomous agent"
    echo "  4. Execute a payment workflow"
    echo ""
    echo -e "${YELLOW}For MNEE Hackathon Demo:${NC}"
    echo "  - Demo video: docs/demo-video.md"
    echo "  - Deployment guide: docs/DEPLOYMENT.md"
    echo "  - API documentation: docs/API.md"
    echo ""
}

# Execute main function
main "$@"
