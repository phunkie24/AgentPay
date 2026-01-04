using AgentPay.Domain.Repositories;
using System.Text.Json;

namespace AgentPay.MCP.Services;

/// <summary>
/// Provides MCP resources (read-only data) to AI agents
/// </summary>
public class MCPResourceProvider
{
    private readonly IAgentRepository _agentRepo;
    private readonly ITransactionRepository _transactionRepo;
    private readonly IServiceRepository _serviceRepo;
    private readonly ILogger<MCPResourceProvider> _logger;

    public MCPResourceProvider(
        IAgentRepository agentRepo,
        ITransactionRepository transactionRepo,
        IServiceRepository serviceRepo,
        ILogger<MCPResourceProvider> logger)
    {
        _agentRepo = agentRepo;
        _transactionRepo = transactionRepo;
        _serviceRepo = serviceRepo;
        _logger = logger;
    }

    public Task<IEnumerable<MCPResource>> GetAllResourcesAsync()
    {
        var resources = new List<MCPResource>
        {
            new MCPResource(
                Uri: "agentpay://agents/active",
                Name: "Active Agents",
                Description: "List of all active agents in the system",
                MimeType: "application/json"
            ),

            new MCPResource(
                Uri: "agentpay://transactions/pending",
                Name: "Pending Transactions",
                Description: "All pending blockchain transactions",
                MimeType: "application/json"
            ),

            new MCPResource(
                Uri: "agentpay://services/active",
                Name: "Active Services",
                Description: "List of all active services available for agents",
                MimeType: "application/json"
            ),

            new MCPResource(
                Uri: "agentpay://system/stats",
                Name: "System Statistics",
                Description: "Overall system statistics and metrics",
                MimeType: "application/json"
            ),

            new MCPResource(
                Uri: "agentpay://blockchain/info",
                Name: "Blockchain Information",
                Description: "Current blockchain network information",
                MimeType: "application/json"
            )
        };

        return Task.FromResult<IEnumerable<MCPResource>>(resources);
    }

    public async Task<string> ReadResourceAsync(string uri)
    {
        _logger.LogInformation("Reading resource: {Uri}", uri);

        return uri switch
        {
            "agentpay://agents/active" => await GetActiveAgentsAsync(),
            "agentpay://transactions/pending" => await GetPendingTransactionsAsync(),
            "agentpay://services/active" => await GetActiveServicesAsync(),
            "agentpay://system/stats" => await GetSystemStatsAsync(),
            "agentpay://blockchain/info" => await GetBlockchainInfoAsync(),
            _ => throw new ArgumentException($"Unknown resource URI: {uri}")
        };
    }

    private async Task<string> GetActiveAgentsAsync()
    {
        // For now, return empty array - will be implemented when repository methods are available
        var data = new
        {
            agents = new object[] { },
            count = 0,
            timestamp = DateTime.UtcNow
        };

        return JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
    }

    private async Task<string> GetPendingTransactionsAsync()
    {
        var transactions = await _transactionRepo.GetPendingTransactionsAsync();
        var data = new
        {
            transactions = transactions.Select(t => new
            {
                id = t.Id,
                agentId = t.AgentId,
                amount = t.Amount.Value,
                status = t.Status.ToString(),
                fromAddress = t.FromAddress.Value,
                toAddress = t.ToAddress.Value,
                initiatedAt = t.InitiatedAt
            }),
            count = transactions.Count(),
            timestamp = DateTime.UtcNow
        };

        return JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
    }

    private async Task<string> GetActiveServicesAsync()
    {
        var services = await _serviceRepo.GetActiveServicesAsync();
        var data = new
        {
            services = services.Select(s => new
            {
                id = s.Id,
                name = s.Name,
                description = s.Description,
                listedPrice = s.ListedPrice.Value,
                category = s.Category.ToString(),
                isActive = s.IsActive
            }),
            count = services.Count(),
            timestamp = DateTime.UtcNow
        };

        return JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
    }

    private Task<string> GetSystemStatsAsync()
    {
        var stats = new
        {
            totalAgents = 0,
            activeAgents = 0,
            totalTransactions = 0,
            pendingTransactions = 0,
            totalServices = 0,
            activeServices = 0,
            uptime = TimeSpan.Zero,
            timestamp = DateTime.UtcNow
        };

        return Task.FromResult(JsonSerializer.Serialize(stats, new JsonSerializerOptions { WriteIndented = true }));
    }

    private Task<string> GetBlockchainInfoAsync()
    {
        var info = new
        {
            network = "Ethereum Mainnet",
            chainId = 1,
            contractAddress = "0x8ccedbAe4916b79da7F3F612EfB2EB93A2bFD6cF",
            tokenSymbol = "MNEE",
            tokenName = "MNEE Token",
            timestamp = DateTime.UtcNow
        };

        return Task.FromResult(JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true }));
    }
}

// MCP Resource Model
public record MCPResource(string Uri, string Name, string Description, string MimeType);
