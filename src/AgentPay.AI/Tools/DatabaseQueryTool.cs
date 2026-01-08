using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.Domain.Repositories;

namespace AgentPay.AI.Tools;

/// <summary>
/// Tool for querying database information
/// </summary>
public class DatabaseQueryTool : ITool
{
    private readonly IAgentRepository _agentRepo;
    private readonly ITransactionRepository _transactionRepo;
    private readonly IServiceRepository _serviceRepo;

    public string Name => "database_query";
    public string Description => "Query agents, transactions, and services from the database";

    public DatabaseQueryTool(
        IAgentRepository agentRepo,
        ITransactionRepository transactionRepo,
        IServiceRepository serviceRepo)
    {
        _agentRepo = agentRepo;
        _transactionRepo = transactionRepo;
        _serviceRepo = serviceRepo;
    }

    public async Task<object> ExecuteAsync(Dictionary<string, object> parameters)
    {
        var queryType = parameters.GetValueOrDefault("query_type")?.ToString();

        return queryType switch
        {
            "agent" => await QueryAgentAsync(parameters),
            "transactions" => await QueryTransactionsAsync(parameters),
            "services" => await QueryServicesAsync(parameters),
            "agent_transactions" => await QueryAgentTransactionsAsync(parameters),
            _ => throw new ArgumentException($"Unknown query type: {queryType}")
        };
    }

    private async Task<object> QueryAgentAsync(Dictionary<string, object> parameters)
    {
        var agentId = Guid.Parse(parameters.GetValueOrDefault("agent_id")?.ToString() ?? throw new ArgumentException("agent_id required"));

        var agent = await _agentRepo.GetByIdAsync(agentId);
        if (agent == null)
            return new { error = "Agent not found" };

        return new
        {
            id = agent.Id,
            name = agent.Name,
            role = agent.Role.ToString(),
            walletAddress = agent.WalletAddress.Value,
            status = agent.Status.ToString(),
            createdAt = agent.CreatedAt
        };
    }

    private async Task<object> QueryTransactionsAsync(Dictionary<string, object> parameters)
    {
        var limit = int.Parse(parameters.GetValueOrDefault("limit")?.ToString() ?? "10");
        var status = parameters.GetValueOrDefault("status")?.ToString();

        var transactions = await _transactionRepo.GetPendingTransactionsAsync();

        return transactions.Select(t => new
        {
            id = t.Id,
            amount = t.Amount.Value,
            status = t.Status.ToString(),
            from = t.FromAddress.Value,
            to = t.ToAddress.Value,
            createdAt = t.InitiatedAt
        }).ToList();
    }

    private async Task<object> QueryServicesAsync(Dictionary<string, object> parameters)
    {
        var services = await _serviceRepo.GetActiveServicesAsync();

        return services.Select(s => new
        {
            id = s.Id,
            name = s.Name,
            description = s.Description,
            pricePerRequest = s.ListedPrice.Value,
            isActive = s.IsActive
        }).ToList();
    }

    private async Task<object> QueryAgentTransactionsAsync(Dictionary<string, object> parameters)
    {
        var agentId = Guid.Parse(parameters.GetValueOrDefault("agent_id")?.ToString() ?? throw new ArgumentException("agent_id required"));

        var transactions = await _transactionRepo.GetByAgentIdAsync(agentId);

        var transactionList = transactions.ToList();

        return new
        {
            agentId,
            totalTransactions = transactionList.Count,
            transactions = transactionList.Select(t => new
            {
                id = t.Id,
                amount = t.Amount.Value,
                status = t.Status.ToString(),
                createdAt = t.InitiatedAt
            }).ToList()
        };
    }
}
