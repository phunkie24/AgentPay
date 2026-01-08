using Microsoft.AspNetCore.Mvc;
using AgentPay.MCP.Client;

namespace AgentPay.MCP.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentController : ControllerBase
{
    private readonly OllamaMCPClient _mcpClient;
    private readonly ILogger<AgentController> _logger;

    public AgentController(OllamaMCPClient mcpClient, ILogger<AgentController> logger)
    {
        _mcpClient = mcpClient;
        _logger = logger;
    }

    /// <summary>
    /// Execute an AI agent task using Ollama with MCP tools
    /// </summary>
    /// <param name="request">Task request with description and context</param>
    /// <returns>Agent execution result</returns>
    [HttpPost("execute")]
    public async Task<ActionResult<AgentExecutionResult>> ExecuteTask([FromBody] AgentTaskRequest request)
    {
        _logger.LogInformation("Received agent task request: {Task}", request.Task);

        try
        {
            var result = await _mcpClient.ExecuteAgentTaskAsync(request.Task, request.Context);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing agent task: {Task}", request.Task);
            return StatusCode(500, new
            {
                error = "Failed to execute agent task",
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// Get agent status and capabilities
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult> GetStatus()
    {
        try
        {
            var mcpStatus = await _mcpClient.InitializeAsync();

            return Ok(new
            {
                status = "operational",
                backend = "Ollama (Free)",
                mcpServer = mcpStatus,
                capabilities = new[]
                {
                    "Blockchain Payments",
                    "Agent Memory Management",
                    "Database Queries",
                    "Transaction Analysis",
                    "Service Negotiation"
                },
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting agent status");
            return StatusCode(500, new
            {
                status = "error",
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// Example agent tasks for testing
    /// </summary>
    [HttpGet("examples")]
    public ActionResult<List<AgentTaskExample>> GetExamples()
    {
        var examples = new List<AgentTaskExample>
        {
            new AgentTaskExample
            {
                Name = "Check Agent Balance",
                Task = "Query the database for agent information and check their MNEE balance",
                Context = new Dictionary<string, object>
                {
                    ["agent_id"] = "00000000-0000-0000-0000-000000000001"
                },
                ExpectedTools = new[] { "database_query" }
            },

            new AgentTaskExample
            {
                Name = "Create Payment",
                Task = "Create a payment of 10 MNEE tokens to address 0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb1 for AI service usage",
                Context = new Dictionary<string, object>
                {
                    ["agent_id"] = "00000000-0000-0000-0000-000000000001",
                    ["to_address"] = "0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb1",
                    ["amount"] = 10,
                    ["purpose"] = "AI Service Payment"
                },
                ExpectedTools = new[] { "payment", "database_query" }
            },

            new AgentTaskExample
            {
                Name = "Store Agent Memory",
                Task = "Store the fact that I successfully completed a payment transaction in my memory for future reference",
                Context = new Dictionary<string, object>
                {
                    ["agent_id"] = "00000000-0000-0000-0000-000000000001",
                    ["key"] = "last_successful_payment",
                    ["value"] = "Completed 10 MNEE payment on " + DateTime.UtcNow.ToString("yyyy-MM-dd")
                },
                ExpectedTools = new[] { "agent_memory" }
            },

            new AgentTaskExample
            {
                Name = "Query Blockchain",
                Task = "Check the MNEE token balance for wallet address 0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb1",
                Context = new Dictionary<string, object>
                {
                    ["address"] = "0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb1"
                },
                ExpectedTools = new[] { "blockchain_query" }
            },

            new AgentTaskExample
            {
                Name = "Complex Task: Payment with Verification",
                Task = "Create a 5 MNEE payment, store it in memory, and then verify the transaction was created successfully",
                Context = new Dictionary<string, object>
                {
                    ["agent_id"] = "00000000-0000-0000-0000-000000000001",
                    ["to_address"] = "0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb1",
                    ["amount"] = 5
                },
                ExpectedTools = new[] { "payment", "agent_memory", "database_query" }
            }
        };

        return Ok(examples);
    }
}

// Request/Response Models
public class AgentTaskRequest
{
    public string Task { get; set; } = string.Empty;
    public Dictionary<string, object>? Context { get; set; }
}

public class AgentTaskExample
{
    public string Name { get; set; } = string.Empty;
    public string Task { get; set; } = string.Empty;
    public Dictionary<string, object> Context { get; set; } = new();
    public string[] ExpectedTools { get; set; } = Array.Empty<string>();
}
