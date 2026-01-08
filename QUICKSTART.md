# 🚀 AgentPay - Quick Start Guide

## MNEE Hackathon Submission - AI & Agent Payments Track

### What is AgentPay?

AgentPay is a production-ready platform enabling **autonomous AI agents to discover, negotiate, and pay for services** using MNEE stablecoin on Ethereum.

### Key Features

✅ **33 GenAI Patterns** - Most comprehensive implementation  
✅ **Autonomous Payments** - Agents pay without human intervention  
✅ **Multi-Agent Collaboration** - Specialized agents working together  
✅ **MNEE Integration** - Native stablecoin support  
✅ **Enterprise-Grade** - Security, guardrails, monitoring  

---

## 🎯 Quick Start (5 minutes)

### Prerequisites

- Docker & Docker Compose
- 4GB+ RAM
- (Optional) .NET 10.0 SDK for local dev

### Step 1: Clone & Configure

```bash
git clone https://github.com/yourusername/agentpay.git
cd agentpay

# Copy environment template
cp .env.example .env

# Edit .env with your keys (optional for demo)
nano .env
```

### Step 2: Start with Docker

```bash
# Option A: Quick start (pre-configured)
bash scripts/setup.sh quick

# Option B: Full setup (first time)
bash scripts/setup.sh all
```

### Step 3: Access Application

Open your browser to:
- **Dashboard**: http://localhost:5000
- **API Docs**: http://localhost:5000/swagger

---

## 📱 Demo Walkthrough

### 1. Create Your First Agent

```http
POST http://localhost:5000/api/agents
Content-Type: application/json

{
  "name": "PaymentBot",
  "role": "Executor",
  "walletAddress": "0x...",
  "initialBalance": 100.0
}
```

### 2. Execute Autonomous Payment

```http
POST http://localhost:5000/api/payments/autonomous
Content-Type: application/json

{
  "agentId": "{agent-id}",
  "serviceId": "data-api-access",
  "maxBudget": 10.0,
  "objective": "Purchase 30-day API access"
}
```

### 3. Monitor in Real-Time

Navigate to **Dashboard** → **Agents** → Select your agent

Watch as the agent:
1. 🔍 Discovers the service
2. 💭 Plans the payment strategy
3. 🤝 Negotiates the price
4. 💰 Executes MNEE transfer
5. ✅ Verifies transaction
6. 🧠 Reflects and learns

---

## 🏗️ Architecture Overview

```
User → Web (MVC) → Application Layer → Agentic AI → Blockchain
                       ↓                    ↓
                   SQL Server          LLM (Ollama)
                      ↓                    ↓
                   Redis            Vector DB (Qdrant)
```

### Key Components

1. **Web Layer** - ASP.NET MVC + SignalR real-time
2. **Application** - CQRS with MediatR
3. **Agentic AI** - 6 specialized agents implementing 33 GenAI patterns
4. **Blockchain** - Nethereum + MNEE stablecoin
5. **Data** - SQL Server + Redis + Vector DB

---

## 🤖 Agent Types

| Agent | Pattern | Purpose |
|-------|---------|---------|
| **ReAct** | Reason + Act | Service discovery, data retrieval |
| **Planning** | Decomposition + Plan-Execute | Strategic payment workflows |
| **Negotiation** | Debate | Price optimization |
| **Verification** | Self-Check | Transaction validation |
| **Reflection** | Learning | Strategy improvement |
| **Memory** | Long-Term Memory | Context retention |

---

## 💡 GenAI Patterns Used

### Reasoning (5 patterns)
- Chain of Thought
- Decomposition
- Plan-and-Execute
- Step-Back Prompting
- Tree of Thoughts

### Tools & Actions (6 patterns)
- Tool Calling
- Function Calling
- Reflection
- Verification
- Tool-Augmented Reasoning
- LLM-as-Judge

### Multi-Agent (3 patterns)
- Role Prompting
- Multi-Agent Collaboration
- Debate

### Memory (2 patterns)
- Session Memory
- Long-Term Memory

### Safety (2 patterns)
- Self-Check (Hallucination Detection)
- Guardrails (Policy Enforcement)

**+ 15 more patterns** for RAG, context management, prompting, etc.

---

## 🔧 Configuration

### LLM Provider

**Option 1: Ollama (Local, Free)**
```json
{
  "AI": {
    "Provider": "Ollama",
    "BaseUrl": "http://localhost:11434",
    "Model": "llama3.2:latest"
  }
}
```

**Option 2: OpenAI**
```json
{
  "AI": {
    "Provider": "OpenAI",
    "ApiKey": "sk-...",
    "Model": "gpt-4"
  }
}
```

### Blockchain

```json
{
  "Blockchain": {
    "EthereumRpcUrl": "https://eth-mainnet.g.alchemy.com/v2/YOUR_KEY",
    "MNEEContractAddress": "0x8ccedbAe4916b79da7F3F612EfB2EB93A2bFD6cF",
    "ChainId": 1
  }
}
```

---

## 📊 Sample Workflow

### Autonomous Service Purchase

```csharp
// 1. Agent analyzes need
var service = await agent.DiscoverServiceAsync("weather-api");

// 2. Agent creates payment plan
var plan = await agent.PlanPaymentAsync(service, maxBudget: 50);

// 3. Agent negotiates price
var terms = await agent.NegotiateAsync(service, budget: 50);
// Result: Listed $10/month → Negotiated $8/month

// 4. Agent executes payment
var txHash = await agent.PayWithMNEEAsync(terms.Price);

// 5. Agent verifies transaction
var verified = await agent.VerifyAsync(txHash);

// 6. Agent reflects and learns
await agent.ReflectAsync(verified);
// Learning: "Negotiation saved $2. Strategy: effective."
```

---

## 🎥 Demo Video

▶️ **5-Minute Demo**: [Watch on YouTube](#)

Demonstrates:
- Agent creation
- Autonomous payment workflow
- Real-time monitoring
- Multi-agent collaboration
- Reflection & learning

---

## 📚 Documentation

- [Complete Architecture](docs/ARCHITECTURE.md) - System design + all 33 patterns
- [API Reference](docs/API.md) - REST endpoints
- [Agent Development](docs/AGENTS.md) - Creating custom agents
- [Deployment](docs/DEPLOYMENT.md) - Production deployment

---

## 🐛 Troubleshooting

### Services won't start
```bash
docker-compose down
docker system prune -f
docker-compose up -d
```

### Ollama model download fails
```bash
docker-compose exec ollama ollama pull llama3.2:latest
```

### Database connection issues
```bash
docker-compose restart sqlserver
# Wait 30 seconds
docker-compose restart web
```

---

## 🏆 MNEE Hackathon

**Track**: AI & Agent Payments  
**Prize**: $12,500 MNEE Stablecoin

**Why AgentPay Wins:**
1. ✅ **Most Comprehensive** - 33 GenAI patterns
2. ✅ **Production-Ready** - Full deployment, monitoring
3. ✅ **Open Source** - MIT licensed
4. ✅ **Extensible** - MCP protocol, plugin architecture
5. ✅ **Well-Documented** - Complete guides + video

---

## 🤝 Contributing

Contributions welcome! See [CONTRIBUTING.md](CONTRIBUTING.md)

---

## 📄 License

MIT License - see [LICENSE](LICENSE)

---

**Built with ❤️ for the MNEE Hackathon**

*Making money programmable for AI agents*
