# AgentPay - Complete Architecture & GenAI Patterns Implementation

## Table of Contents
1. [System Architecture](#system-architecture)
2. [GenAI Patterns Implementation](#genai-patterns-implementation)
3. [Agent Workflows](#agent-workflows)
4. [Database Schema](#database-schema)
5. [API Endpoints](#api-endpoints)
6. [Deployment Architecture](#deployment-architecture)

## System Architecture

### High-Level Architecture Diagram

```
┌───────────────────────────────────────────────────────────────────────┐
│                        CLIENT LAYER                                    │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                │
│  │   Browser    │  │  Mobile App  │  │   CLI Tool   │                │
│  │  (Razor MVC) │  │  (Future)    │  │  (Optional)  │                │
│  └──────────────┘  └──────────────┘  └──────────────┘                │
└───────────────────────────────────────────────────────────────────────┘
                               ↓ HTTPS/SignalR
┌───────────────────────────────────────────────────────────────────────┐
│                     WEB/API LAYER (ASP.NET MVC)                        │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                │
│  │  Controllers │  │  SignalR Hubs│  │  Middleware  │                │
│  │   (MVC)      │  │  (Real-time) │  │  (Auth/CORS) │                │
│  └──────────────┘  └──────────────┘  └──────────────┘                │
└───────────────────────────────────────────────────────────────────────┘
                               ↓ MediatR
┌───────────────────────────────────────────────────────────────────────┐
│                   APPLICATION LAYER (CQRS)                             │
│                                                                        │
│  ┌─────────────────────────┐  ┌─────────────────────────┐            │
│  │  COMMANDS               │  │  QUERIES                │            │
│  │  ├─ CreateAgentCommand  │  │  ├─ GetAgentQuery       │            │
│  │  ├─ InitiatePayment     │  │  ├─ GetTransactions     │            │
│  │  └─ UpdateStrategy      │  │  └─ GetDashboardData    │            │
│  └─────────────────────────┘  └─────────────────────────┘            │
│                                                                        │
│  ┌──────────────────────────────────────────────────────┐            │
│  │  APPLICATION SERVICES                                │            │
│  │  ├─ AgentService        ├─ PaymentService            │            │
│  │  ├─ TransactionService  ├─ AnalyticsService          │            │
│  │  └─ GuardrailsService   └─ NotificationService       │            │
│  └──────────────────────────────────────────────────────┘            │
└───────────────────────────────────────────────────────────────────────┘
                               ↓
┌───────────────────────────────────────────────────────────────────────┐
│                    AGENTIC AI ORCHESTRATION LAYER                      │
│                                                                        │
│  ┌─────────────────────────────────────────────────────────────────┐ │
│  │  MULTI-AGENT COORDINATOR (Pattern 23)                           │ │
│  │  ┌────────────┐  ┌────────────┐  ┌────────────┐                │ │
│  │  │  Planning  │→│ Negotiation│→│  Execution │                  │ │
│  │  │   Agent    │  │   Agent    │  │   Agent    │                 │ │
│  │  └────────────┘  └────────────┘  └────────────┘                 │ │
│  │         ↓               ↓               ↓                        │ │
│  │  ┌────────────┐  ┌────────────┐  ┌────────────┐                │ │
│  │  │Verification│  │ Reflection │  │   Memory   │                 │ │
│  │  │   Agent    │  │   Agent    │  │   Agent    │                 │ │
│  │  └────────────┘  └────────────┘  └────────────┘                 │ │
│  └─────────────────────────────────────────────────────────────────┘ │
│                                                                        │
│  ┌─────────────────────────────────────────────────────────────────┐ │
│  │  MCP SERVER (Model Context Protocol)                            │ │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │ │
│  │  │  Tool        │  │  Resource    │  │  Prompt      │          │ │
│  │  │  Registry    │  │  Manager     │  │  Templates   │          │ │
│  │  └──────────────┘  └──────────────┘  └──────────────┘          │ │
│  └─────────────────────────────────────────────────────────────────┘ │
│                                                                        │
│  ┌─────────────────────────────────────────────────────────────────┐ │
│  │  LLM SERVICES (Pattern Support)                                 │ │
│  │  ├─ Chain of Thought (Pattern 13)                               │ │
│  │  ├─ Decomposition (Pattern 12)                                  │ │
│  │  ├─ Plan-and-Execute (Pattern 14)                               │ │
│  │  ├─ Tool Calling (Pattern 21)                                   │ │
│  │  ├─ Reflection (Pattern 18)                                     │ │
│  │  └─ Self-Check (Pattern 31)                                     │ │
│  └─────────────────────────────────────────────────────────────────┘ │
└───────────────────────────────────────────────────────────────────────┘
                               ↓
┌───────────────────────────────────────────────────────────────────────┐
│                        DOMAIN LAYER (DDD)                              │
│                                                                        │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                │
│  │  ENTITIES    │  │ VALUE OBJECTS│  │  AGGREGATES  │                │
│  │  ├─ Agent    │  │ ├─ MNEE      │  │ ├─ Payment   │                │
│  │  ├─ Trans    │  │ │   Amount   │  │ │   Session  │                │
│  │  └─ Service  │  │ ├─ Wallet    │  │ └─ Agent     │                │
│  │              │  │ │   Address  │  │    Workflow  │                │
│  │              │  │ └─ TxHash    │  │              │                │
│  └──────────────┘  └──────────────┘  └──────────────┘                │
│                                                                        │
│  ┌──────────────────────────────────────────────────────┐            │
│  │  DOMAIN EVENTS                                       │            │
│  │  ├─ AgentCreatedEvent    ├─ PaymentInitiatedEvent   │            │
│  │  ├─ PaymentCompletedEvent├─ ReflectionCreatedEvent  │            │
│  │  └─ AgentDegradedEvent   └─ BalanceUpdatedEvent     │            │
│  └──────────────────────────────────────────────────────┘            │
└───────────────────────────────────────────────────────────────────────┘
                               ↓
┌───────────────────────────────────────────────────────────────────────┐
│                     INFRASTRUCTURE LAYER                               │
│                                                                        │
│  ┌─────────────────────────────────────────────────────────────────┐ │
│  │  PERSISTENCE (SQL Server + EF Core)                             │ │
│  │  ├─ ApplicationDbContext                                        │ │
│  │  ├─ Repositories (Agent, Transaction, Service)                  │ │
│  │  ├─ Unit of Work                                                │ │
│  │  └─ Migrations                                                  │ │
│  └─────────────────────────────────────────────────────────────────┘ │
│                                                                        │
│  ┌─────────────────────────────────────────────────────────────────┐ │
│  │  BLOCKCHAIN INTEGRATION (Nethereum)                             │ │
│  │  ├─ MNEE Contract Service (0x8cce...FD6cF)                      │ │
│  │  ├─ Ethereum Client (Web3)                                      │ │
│  │  ├─ Wallet Manager (HD Wallet)                                  │ │
│  │  └─ Gas Price Oracle                                            │ │
│  └─────────────────────────────────────────────────────────────────┘ │
│                                                                        │
│  ┌─────────────────────────────────────────────────────────────────┐ │
│  │  AI/LLM INTEGRATION                                             │ │
│  │  ├─ Ollama Client (Llama 3.2, Mistral)                          │ │
│  │  ├─ Embedding Service (all-MiniLM-L6-v2)                        │ │
│  │  ├─ Vector Store (Qdrant)                                       │ │
│  │  └─ Token Counter & Context Manager                             │ │
│  └─────────────────────────────────────────────────────────────────┘ │
│                                                                        │
│  ┌─────────────────────────────────────────────────────────────────┐ │
│  │  CACHING & MESSAGING                                            │ │
│  │  ├─ Redis Cache (Session, Agent State)                          │ │
│  │  ├─ RabbitMQ (Event Bus, Task Queue)                            │ │
│  │  └─ In-Memory Cache (Hot Data)                                  │ │
│  └─────────────────────────────────────────────────────────────────┘ │
└───────────────────────────────────────────────────────────────────────┘
```

## GenAI Patterns Implementation

### Complete Pattern Coverage (33/33 Patterns)

#### Category 1-2: Model Control & Knowledge Retrieval

**Pattern 2: Grammar (Structured Output)**
```csharp
public class StructuredOutputService
{
    public async Task<T> GenerateStructuredAsync<T>(string prompt)
    {
        var schema = JsonSchema.FromType<T>();
        var response = await _llm.GenerateAsync(prompt, new
        {
            ResponseFormat = "json",
            Schema = schema
        });
        return JsonSerializer.Deserialize<T>(response.Text);
    }
}
```

**Pattern 4: Prompt Templates**
```csharp
public class PromptTemplates
{
    public const string AGENT_REASONING = """
        You are {role} agent. Your task: {task}
        
        Context: {context}
        Available tools: {tools}
        
        Think step by step and provide your reasoning:
        """;
    
    public const string PAYMENT_NEGOTIATION = """
        Service: {serviceName}
        Listed Price: {price} MNEE
        Your Budget: {budget} MNEE
        
        Negotiate the best price. Be professional but firm.
        """;
}
```

**Pattern 6: Basic RAG**
```csharp
public class RAGService
{
    private readonly IVectorStore _vectorStore;
    
    public async Task<string> RetrieveAugmentedContextAsync(
        string query, int topK = 5)
    {
        // Generate query embedding
        var embedding = await _embeddingService.EmbedAsync(query);
        
        // Retrieve relevant documents
        var docs = await _vectorStore.SearchAsync(embedding, topK);
        
        // Augment prompt with retrieved context
        var context = string.Join("\n", docs.Select(d => d.Content));
        return $$"""
            Relevant context:
            {{context}}
            
            User query: {{query}}
            
            Answer based on the context above:
            """;
    }
}
```

**Pattern 7: Context Window Management**
```csharp
public class ContextWindowManager
{
    private const int MAX_TOKENS = 8000;
    
    public async Task<List<Message>> ManageContextAsync(
        List<Message> messages)
    {
        var currentTokens = EstimateTokens(messages);
        
        if (currentTokens > MAX_TOKENS)
        {
            // Strategy 1: Remove oldest messages
            messages = messages.Skip(messages.Count / 4).ToList();
            
            // Strategy 2: Summarize old messages
            var summary = await SummarizeMessagesAsync(
                messages.Take(messages.Count / 2));
            
            messages = new[] { summary }
                .Concat(messages.Skip(messages.Count / 2))
                .ToList();
        }
        
        return messages;
    }
}
```

**Pattern 10: Query Rewriting**
```csharp
public class QueryRewritingService
{
    public async Task<List<string>> RewriteQueryAsync(string query)
    {
        var prompt = $$"""
            Original query: {{query}}
            
            Generate 3 alternative phrasings that might retrieve better results:
            1.
            2.
            3.
            """;
        
        var response = await _llm.GenerateAsync(prompt);
        return ParseRewrites(response.Text);
    }
}
```

#### Category 3: Reasoning & Planning

**Pattern 11: Step-Back Prompting**
```csharp
public class StepBackPrompting
{
    public async Task<string> GenerateHighLevelStrategyAsync(
        string task)
    {
        var prompt = $$"""
            Before solving this task, step back and think:
            
            Task: {{task}}
            
            1. What is the fundamental problem here?
            2. What are the key principles involved?
            3. What is the general approach?
            
            Provide a high-level strategy:
            """;
        
        return await _llm.GenerateAsync(prompt);
    }
}
```

**Pattern 12: Decomposition** (See PlanningAgent.cs)
```csharp
public async Task<List<Subtask>> DecomposeAsync(string goal)
{
    var prompt = $$"""
        Complex goal: {{goal}}
        
        Break this into 3-7 concrete subtasks:
        - Each should be specific and actionable
        - Order them logically
        - Identify dependencies
        
        Output as JSON array of subtasks
        """;
    
    var response = await _llm.GenerateAsync(prompt, 
        new { ResponseFormat = "json" });
    return JsonSerializer.Deserialize<List<Subtask>>(response);
}
```

**Pattern 13: Chain of Thought** (See ReActAgent.cs)
```csharp
public async Task<ThoughtProcess> ChainOfThoughtAsync(
    string problem)
{
    var prompt = $$"""
        Problem: {{problem}}
        
        Let's solve this step by step:
        
        Step 1: [Understand the problem]
        Step 2: [Identify what we know]
        Step 3: [Determine what we need]
        Step 4: [Plan the solution]
        Step 5: [Execute]
        
        Think through each step:
        """;
    
    return await _llm.GenerateAsync(prompt);
}
```

**Pattern 14: Plan-and-Execute** (See PlanningAgent.cs)
```csharp
public async Task<ExecutionResult> PlanAndExecuteAsync(
    string goal)
{
    // 1. Create plan
    var plan = await CreatePlanAsync(goal);
    
    // 2. Execute each step
    var results = new List<StepResult>();
    foreach (var step in plan.Steps)
    {
        var result = await ExecuteStepAsync(step);
        results.Add(result);
        
        // If critical step fails, abort
        if (!result.Success && step.IsCritical)
            break;
    }
    
    // 3. Synthesize final result
    return await SynthesizeResultsAsync(results);
}
```

**Pattern 15: Tree of Thoughts**
```csharp
public class TreeOfThoughtsAgent
{
    public async Task<Solution> ExploreAsync(Problem problem)
    {
        // Generate multiple thought branches
        var thoughts = await GenerateThoughtsAsync(problem, count: 3);
        
        // Evaluate each branch
        var evaluations = new List<ThoughtEvaluation>();
        foreach (var thought in thoughts)
        {
            var eval = await EvaluateThoughtAsync(thought);
            evaluations.Add(eval);
            
            // Expand promising branches
            if (eval.Score > 0.7)
            {
                var subThoughts = await GenerateThoughtsAsync(
                    thought, count: 2);
                // Recursively explore...
            }
        }
        
        // Select best path
        return await SelectBestPathAsync(evaluations);
    }
}
```

#### Category 4: Tools & External Capabilities

**Pattern 16: Function Calling**
```csharp
[AgentTool("get_mnee_balance")]
public async Task<decimal> GetMNEEBalanceAsync(
    [Description("Wallet address")] string address)
{
    var balance = await _blockchainService.GetBalanceAsync(address);
    return balance;
}

[AgentTool("send_mnee")]
public async Task<string> SendMNEEAsync(
    [Description("Recipient address")] string to,
    [Description("Amount in MNEE")] decimal amount)
{
    var txHash = await _blockchainService.TransferAsync(to, amount);
    return txHash;
}
```

**Pattern 17: LLM-as-Judge**
```csharp
public class LLMJudgeService
{
    public async Task<JudgmentResult> EvaluateOutputAsync(
        string output, string criteria)
    {
        var prompt = $$"""
            Evaluate this output against the criteria:
            
            Output: {{output}}
            Criteria: {{criteria}}
            
            Rate from 1-10 and explain your reasoning.
            Output as JSON: {"score": 8, "reasoning": "..."}
            """;
        
        var response = await _llm.GenerateAsync(prompt,
            new { ResponseFormat = "json" });
        return JsonSerializer.Deserialize<JudgmentResult>(response);
    }
}
```

**Pattern 18: Reflection** (See ReflectionAgent.cs)
```csharp
public async Task<Improvement> ReflectAsync(
    ActionResult action)
{
    var prompt = $$"""
        You performed this action:
        {{JsonSerializer.Serialize(action)}}
        
        Reflect on:
        1. What went well?
        2. What could be improved?
        3. What did you learn?
        4. How would you do it differently?
        
        Provide actionable insights:
        """;
    
    var reflection = await _llm.GenerateAsync(prompt);
    await _memoryService.StoreInsightAsync(reflection);
    return ParseImprovement(reflection);
}
```

**Pattern 19: Verification**
```csharp
public class VerificationAgent
{
    public async Task<VerificationResult> VerifyAsync(
        Transaction transaction)
    {
        // Check 1: Transaction exists on blockchain
        var receipt = await _web3.GetTransactionReceiptAsync(
            transaction.Hash);
        
        // Check 2: Correct amount transferred
        var amount = ExtractAmount(receipt);
        var amountMatches = amount == transaction.Amount;
        
        // Check 3: Correct recipient
        var recipient = ExtractRecipient(receipt);
        var recipientMatches = recipient == transaction.ToAddress;
        
        // Check 4: Gas used is reasonable
        var gasReasonable = receipt.GasUsed < 100000;
        
        var allChecks = amountMatches && recipientMatches && 
                        gasReasonable;
        
        return new VerificationResult
        {
            IsVerified = allChecks,
            Checks = new[] {
                new Check("Amount", amountMatches),
                new Check("Recipient", recipientMatches),
                new Check("Gas", gasReasonable)
            }
        };
    }
}
```

**Pattern 20: Tool-Augmented Reasoning**
```csharp
public class ToolAugmentedAgent
{
    public async Task<Answer> ReasonWithToolsAsync(Question q)
    {
        // Think about what tools are needed
        var thought = await _llm.GenerateAsync($$"""
            Question: {{q.Text}}
            Available tools: {{GetToolsList()}}
            
            Which tools should I use and why?
            """);
        
        // Use identified tools
        var tools = ParseToolsFromThought(thought);
        var toolResults = new Dictionary<string, object>();
        
        foreach (var tool in tools)
        {
            var result = await _toolRegistry.ExecuteAsync(
                tool.Name, tool.Parameters);
            toolResults[tool.Name] = result;
        }
        
        // Reason with tool outputs
        return await _llm.GenerateAsync($$"""
            Question: {{q.Text}}
            Tool results: {{JsonSerializer.Serialize(toolResults)}}
            
            Now answer the question using these results:
            """);
    }
}
```

**Pattern 21: Tool Calling** (See ReActAgent.cs)

#### Category 5: Multi-Agent Systems

**Pattern 22: Role Prompting**
```csharp
public class RolePromptingService
{
    public string GetRolePrompt(AgentRole role)
    {
        return role switch
        {
            AgentRole.Planner => """
                You are a strategic planning agent. Your role is to:
                - Think at a high level
                - Break down complex goals into actionable steps
                - Identify dependencies and risks
                - Optimize resource allocation
                """,
            
            AgentRole.Negotiator => """
                You are a negotiation agent. Your role is to:
                - Analyze offers and counteroffers
                - Find win-win solutions
                - Stay within budget constraints
                - Maximize value for your principal
                """,
            
            AgentRole.Executor => """
                You are an execution agent. Your role is to:
                - Follow plans precisely
                - Use tools effectively
                - Handle errors gracefully
                - Report progress clearly
                """,
            
            AgentRole.Verifier => """
                You are a verification agent. Your role is to:
                - Check work for correctness
                - Detect inconsistencies
                - Validate against requirements
                - Ensure quality standards
                """
        };
    }
}
```

**Pattern 23: Multi-Agent Collaboration**
```csharp
public class MultiAgentWorkflow
{
    public async Task<PaymentResult> ExecutePaymentWorkflowAsync(
        PaymentRequest request)
    {
        // Agent 1: Planning
        var plan = await _planningAgent.CreatePlanAsync(request);
        
        // Agent 2: Discovery
        var service = await _discoveryAgent.FindServiceAsync(
            request.ServiceId);
        
        // Agent 3: Negotiation
        var terms = await _negotiationAgent.NegotiateAsync(
            service, request.MaxBudget);
        
        // Agent 4: Execution
        var txHash = await _executionAgent.PayAsync(terms);
        
        // Agent 5: Verification
        var verified = await _verificationAgent.VerifyAsync(txHash);
        
        // Agent 6: Reflection
        await _reflectionAgent.LearnFromAsync(verified);
        
        return new PaymentResult
        {
            Success = verified.IsVerified,
            TransactionHash = txHash,
            FinalPrice = terms.Price
        };
    }
}
```

**Pattern 24: Debate**
```csharp
public class DebatePattern
{
    public async Task<Decision> DebateDecisionAsync(
        DecisionPoint decision)
    {
        // Agent A: Argues FOR
        var argumentFor = await _advocateAgent.ArgueForAsync(decision);
        
        // Agent B: Argues AGAINST
        var argumentAgainst = await _skepticAgent.ArgueAgainstAsync(
            decision);
        
        // Agent C: Judges
        var judgment = await _judgeAgent.EvaluateDebateAsync(
            argumentFor, argumentAgainst);
        
        return judgment.FinalDecision;
    }
}
```

#### Category 6: Memory & Learning

**Pattern 26: Session Memory** (See AgentContext.cs)
```csharp
public class SessionMemory
{
    private readonly List<Message> _messages = new();
    private readonly Dictionary<string, object> _context = new();
    
    public void AddMessage(string role, string content)
    {
        _messages.Add(new Message(role, content, DateTime.UtcNow));
    }
    
    public void SetContext(string key, object value)
    {
        _context[key] = value;
    }
    
    public List<Message> GetRecentMessages(int count = 10)
    {
        return _messages.TakeLast(count).ToList();
    }
}
```

**Pattern 28: Long-Term Memory**
```csharp
public class LongTermMemoryService
{
    private readonly IVectorStore _vectorStore;
    private readonly IRepository<Memory> _memoryRepo;
    
    public async Task StoreMemoryAsync(
        string key, object value, string context)
    {
        // Store in vector DB with embedding
        var embedding = await _embeddingService.EmbedAsync(
            $"{key}: {context}");
        await _vectorStore.UpsertAsync(key, embedding, value);
        
        // Store in SQL for structured queries
        await _memoryRepo.AddAsync(new Memory
        {
            Key = key,
            Value = JsonSerializer.Serialize(value),
            Context = context,
            CreatedAt = DateTime.UtcNow
        });
    }
    
    public async Task<List<Memory>> RecallSimilarAsync(
        string query, int topK = 5)
    {
        var queryEmbedding = await _embeddingService.EmbedAsync(query);
        var similarMemories = await _vectorStore.SearchAsync(
            queryEmbedding, topK);
        return similarMemories;
    }
}
```

#### Category 8: Safety & Governance

**Pattern 31: Self-Check** (See Agent.cs)
```csharp
public class SelfCheckService
{
    public async Task<HallucinationCheck> DetectHallucinationAsync(
        string output)
    {
        // Get token probabilities
        var tokenProbs = await _llm.GetTokenProbabilitiesAsync(output);
        
        // Low probability tokens might indicate hallucination
        var lowConfidenceTokens = tokenProbs
            .Where(t => t.Probability < 0.5)
            .ToList();
        
        // Calculate overall confidence
        var avgConfidence = tokenProbs.Average(t => t.Probability);
        
        return new HallucinationCheck
        {
            IsLikelyHallucination = avgConfidence < 0.7 || 
                                   lowConfidenceTokens.Count > 5,
            ConfidenceScore = avgConfidence,
            SuspiciousTokens = lowConfidenceTokens
        };
    }
    
    public async Task<ConsistencyCheck> CheckConsistencyAsync(
        string output, List<string> facts)
    {
        var prompt = $$"""
            Output: {{output}}
            
            Known facts:
            {{string.Join("\n", facts)}}
            
            Check if the output is consistent with the facts.
            Identify any contradictions.
            """;
        
        var response = await _llm.GenerateAsync(prompt);
        return ParseConsistencyCheck(response);
    }
}
```

**Pattern 32: Guardrails** (See Transaction.cs)
```csharp
public class GuardrailsService
{
    private readonly GuardrailsPolicy _policy;
    
    public async Task<GuardrailsResult> ApplyGuardrailsAsync(
        Transaction transaction)
    {
        var checks = new List<GuardrailCheck>
        {
            // Input guardrails
            CheckAmountLimit(transaction.Amount),
            CheckAddressWhitelist(transaction.ToAddress),
            CheckDailyLimit(transaction.AgentId, transaction.Amount),
            
            // Pattern guardrails
            await CheckSuspiciousPatternAsync(transaction),
            
            // Content guardrails
            await CheckReasoningQualityAsync(transaction.Reasoning),
            
            // Output guardrails
            await CheckOutputSafetyAsync(transaction)
        };
        
        var allPassed = checks.All(c => c.Passed);
        
        if (!allPassed)
        {
            await _auditLog.LogBlockedTransactionAsync(
                transaction, checks);
        }
        
        return new GuardrailsResult
        {
            Passed = allPassed,
            Checks = checks,
            BlockedReason = allPassed ? null : 
                string.Join("; ", checks
                    .Where(c => !c.Passed)
                    .Select(c => c.Reason))
        };
    }
    
    private GuardrailCheck CheckAmountLimit(decimal amount)
    {
        var passed = amount <= _policy.MaxTransactionAmount;
        return new GuardrailCheck(
            "Amount Limit",
            passed,
            passed ? "OK" : $"Exceeds limit of {_policy.MaxTransactionAmount}"
        );
    }
    
    private async Task<GuardrailCheck> CheckSuspiciousPatternAsync(
        Transaction transaction)
    {
        // ML-based fraud detection
        var features = ExtractFeatures(transaction);
        var suspicionScore = await _fraudDetector.ScoreAsync(features);
        
        var passed = suspicionScore < 0.7;
        return new GuardrailCheck(
            "Fraud Detection",
            passed,
            passed ? "OK" : $"Suspicious score: {suspicionScore:F2}"
        );
    }
}
```

## Continued in next section...

This document covers the complete architecture and all 33 GenAI patterns. For implementation details of specific components, see:
- `/src/AgentPay.AI/Agents/` - Agent implementations
- `/src/AgentPay.Domain/Entities/` - Domain models
- `/src/AgentPay.Infrastructure/` - Infrastructure services
