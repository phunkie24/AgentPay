using AgentPay.AI.Tools;
using AgentPay.Domain.Repositories;

namespace AgentPay.MCP.Services;

/// <summary>
/// Registry for MCP tools that wraps AgentPay AI Tools
/// </summary>
public class MCPToolRegistry
{
    private readonly IAgentRepository _agentRepo;
    private readonly ITransactionRepository _transactionRepo;
    private readonly IServiceRepository _serviceRepo;
    private readonly ILogger<MCPToolRegistry> _logger;

    public MCPToolRegistry(
        IAgentRepository agentRepo,
        ITransactionRepository transactionRepo,
        IServiceRepository serviceRepo,
        ILogger<MCPToolRegistry> logger)
    {
        _agentRepo = agentRepo;
        _transactionRepo = transactionRepo;
        _serviceRepo = serviceRepo;
        _logger = logger;
    }

    public Task<IEnumerable<MCPTool>> GetAllToolsAsync()
    {
        var tools = new List<MCPTool>
        {
            new MCPTool(
                Name: "agent_memory",
                Description: "Store and retrieve information from agent memory",
                InputSchema: new
                {
                    type = "object",
                    properties = new
                    {
                        action = new { type = "string", description = "Action: store, retrieve, add_thought, get_thoughts", @enum = new[] { "store", "retrieve", "add_thought", "get_thoughts" } },
                        agent_id = new { type = "string", description = "Agent ID (GUID)" },
                        key = new { type = "string", description = "Memory key (for store action)" },
                        value = new { type = "string", description = "Memory value (for store action)" },
                        thought = new { type = "string", description = "Thought content (for add_thought action)" },
                        reasoning = new { type = "string", description = "Thought reasoning (for add_thought action)" },
                        count = new { type = "integer", description = "Number of thoughts to retrieve (for get_thoughts action)" }
                    },
                    required = new[] { "action", "agent_id" }
                }
            ),

            new MCPTool(
                Name: "payment",
                Description: "Create, execute, and manage MNEE token payments",
                InputSchema: new
                {
                    type = "object",
                    properties = new
                    {
                        action = new { type = "string", description = "Action: create_payment, get_payment_status, cancel_payment", @enum = new[] { "create_payment", "get_payment_status", "cancel_payment" } },
                        agent_id = new { type = "string", description = "Agent ID (GUID)" },
                        to_address = new { type = "string", description = "Recipient wallet address (for create_payment)" },
                        amount = new { type = "number", description = "Payment amount in MNEE tokens (for create_payment)" },
                        purpose = new { type = "string", description = "Payment purpose/description (for create_payment)" },
                        transaction_id = new { type = "string", description = "Transaction ID (GUID, for get_payment_status and cancel_payment)" }
                    },
                    required = new[] { "action" }
                }
            ),

            new MCPTool(
                Name: "database_query",
                Description: "Query agents, transactions, and services from the database",
                InputSchema: new
                {
                    type = "object",
                    properties = new
                    {
                        query_type = new { type = "string", description = "Query type: agent, transactions, services, agent_transactions", @enum = new[] { "agent", "transactions", "services", "agent_transactions" } },
                        agent_id = new { type = "string", description = "Agent ID (GUID, for agent and agent_transactions queries)" },
                        limit = new { type = "integer", description = "Number of records to return (for transactions query)" },
                        status = new { type = "string", description = "Transaction status filter (for transactions query)" }
                    },
                    required = new[] { "query_type" }
                }
            ),

            new MCPTool(
                Name: "blockchain_query",
                Description: "Query blockchain information and smart contracts",
                InputSchema: new
                {
                    type = "object",
                    properties = new
                    {
                        action = new { type = "string", description = "Action: get_balance, get_transaction, estimate_gas", @enum = new[] { "get_balance", "get_transaction", "estimate_gas" } },
                        address = new { type = "string", description = "Wallet address (for get_balance)" },
                        transaction_hash = new { type = "string", description = "Transaction hash (for get_transaction)" },
                        from_address = new { type = "string", description = "From address (for estimate_gas)" },
                        to_address = new { type = "string", description = "To address (for estimate_gas)" },
                        amount = new { type = "number", description = "Amount (for estimate_gas)" }
                    },
                    required = new[] { "action" }
                }
            )
        };

        return Task.FromResult<IEnumerable<MCPTool>>(tools);
    }

    public async Task<object> ExecuteToolAsync(string toolName, Dictionary<string, object> arguments)
    {
        _logger.LogInformation("Executing MCP tool: {ToolName}", toolName);

        ITool tool = toolName switch
        {
            "agent_memory" => new AgentMemoryTool(_agentRepo),
            "payment" => new PaymentTool(_transactionRepo, _agentRepo),
            "database_query" => new DatabaseQueryTool(_agentRepo, _transactionRepo, _serviceRepo),
            "blockchain_query" => new BlockchainQueryTool(null!), // TODO: Inject blockchain service
            _ => throw new ArgumentException($"Unknown tool: {toolName}")
        };

        return await tool.ExecuteAsync(arguments);
    }
}

// MCP Tool Model
public record MCPTool(string Name, string Description, object InputSchema);
