#!/bin/bash

# Script to generate complete AgentPay codebase with all GenAI patterns

BASE_DIR="/home/claude/AgentPay"

echo "🚀 Generating AgentPay - Complete Codebase with GenAI Patterns"
echo "=============================================================="

# Create directory structure
echo "📁 Creating directory structure..."

directories=(
    "src/AgentPay.Web/Controllers"
    "src/AgentPay.Web/Views/Home"
    "src/AgentPay.Web/Views/Agent"
    "src/AgentPay.Web/Views/Dashboard"
    "src/AgentPay.Web/Views/Shared"
    "src/AgentPay.Web/wwwroot/css"
    "src/AgentPay.Web/wwwroot/js"
    "src/AgentPay.Web/Models"
    "src/AgentPay.Application/Services"
    "src/AgentPay.Application/Commands"
    "src/AgentPay.Application/Queries"
    "src/AgentPay.Application/Handlers"
    "src/AgentPay.Application/DTOs"
    "src/AgentPay.Domain/Entities"
    "src/AgentPay.Domain/ValueObjects"
    "src/AgentPay.Domain/Events"
    "src/AgentPay.Domain/Repositories"
    "src/AgentPay.Domain/Exceptions"
    "src/AgentPay.Infrastructure/Persistence"
    "src/AgentPay.Infrastructure/Persistence/Configurations"
    "src/AgentPay.Infrastructure/Persistence/Repositories"
    "src/AgentPay.Infrastructure/Blockchain"
    "src/AgentPay.Infrastructure/Caching"
    "src/AgentPay.Infrastructure/AI"
    "src/AgentPay.AI/Agents"
    "src/AgentPay.AI/Agents/Base"
    "src/AgentPay.AI/Orchestration"
    "src/AgentPay.AI/Services"
    "src/AgentPay.AI/Tools"
    "src/AgentPay.AI/Memory"
    "src/AgentPay.AI/Patterns"
    "src/AgentPay.MCP/Server"
    "src/AgentPay.MCP/Client"
    "src/AgentPay.Shared/Constants"
    "src/AgentPay.Shared/Extensions"
    "src/AgentPay.Shared/Models"
    "tests/AgentPay.UnitTests"
    "docker"
    "scripts"
    "docs"
)

for dir in "${directories[@]}"; do
    mkdir -p "$BASE_DIR/$dir"
done

echo "✅ Directory structure created"
echo ""

