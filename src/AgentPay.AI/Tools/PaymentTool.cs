using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.Domain.Entities;
using AgentPay.Domain.Repositories;
using AgentPay.Domain.ValueObjects;

namespace AgentPay.AI.Tools;

/// <summary>
/// Tool for initiating and managing payments
/// </summary>
public class PaymentTool : ITool
{
    private readonly ITransactionRepository _transactionRepo;
    private readonly IAgentRepository _agentRepo;

    public string Name => "payment";
    public string Description => "Create, execute, and manage MNEE token payments";

    public PaymentTool(
        ITransactionRepository transactionRepo,
        IAgentRepository agentRepo)
    {
        _transactionRepo = transactionRepo;
        _agentRepo = agentRepo;
    }

    public async Task<object> ExecuteAsync(Dictionary<string, object> parameters)
    {
        var action = parameters.GetValueOrDefault("action")?.ToString();

        return action switch
        {
            "create_payment" => await CreatePaymentAsync(parameters),
            "get_payment_status" => await GetPaymentStatusAsync(parameters),
            "cancel_payment" => await CancelPaymentAsync(parameters),
            _ => throw new ArgumentException($"Unknown action: {action}")
        };
    }

    private async Task<object> CreatePaymentAsync(Dictionary<string, object> parameters)
    {
        var agentId = Guid.Parse(parameters.GetValueOrDefault("agent_id")?.ToString() ?? throw new ArgumentException("agent_id required"));
        var toAddress = parameters.GetValueOrDefault("to_address")?.ToString() ?? throw new ArgumentException("to_address required");
        var amount = decimal.Parse(parameters.GetValueOrDefault("amount")?.ToString() ?? throw new ArgumentException("amount required"));
        var purpose = parameters.GetValueOrDefault("purpose")?.ToString() ?? "AI Service Payment";

        var agent = await _agentRepo.GetByIdAsync(agentId);
        if (agent == null)
            throw new ArgumentException("Agent not found");

        // Create transaction using static Initiate method
        var serviceId = parameters.ContainsKey("service_id")
            ? Guid.Parse(parameters["service_id"].ToString()!)
            : Guid.Empty; // Default to empty if not provided

        var transaction = Transaction.Initiate(
            agentId: agent.Id,
            serviceId: serviceId,
            amount: MNEEAmount.Create(amount),
            fromAddress: agent.WalletAddress,
            toAddress: WalletAddress.Create(toAddress),
            reasoning: purpose
        );

        await _transactionRepo.CreateAsync(transaction);

        return new
        {
            transactionId = transaction.Id,
            amount = transaction.Amount.Value,
            status = transaction.Status.ToString(),
            createdAt = transaction.InitiatedAt
        };
    }

    private async Task<object> GetPaymentStatusAsync(Dictionary<string, object> parameters)
    {
        var transactionId = Guid.Parse(parameters.GetValueOrDefault("transaction_id")?.ToString() ?? throw new ArgumentException("transaction_id required"));

        var transaction = await _transactionRepo.GetByIdAsync(transactionId);
        if (transaction == null)
            return new { error = "Transaction not found" };

        return new
        {
            transactionId = transaction.Id,
            status = transaction.Status.ToString(),
            amount = transaction.Amount.Value,
            from = transaction.FromAddress.Value,
            to = transaction.ToAddress.Value,
            transactionHash = transaction.Hash?.Value,
            createdAt = transaction.InitiatedAt,
            completedAt = transaction.CompletedAt
        };
    }

    private async Task<object> CancelPaymentAsync(Dictionary<string, object> parameters)
    {
        var transactionId = Guid.Parse(parameters.GetValueOrDefault("transaction_id")?.ToString() ?? throw new ArgumentException("transaction_id required"));

        var transaction = await _transactionRepo.GetByIdAsync(transactionId);
        if (transaction == null)
            return new { error = "Transaction not found" };

        transaction.Fail("Cancelled by agent");
        await _transactionRepo.UpdateAsync(transaction);

        return new
        {
            transactionId = transaction.Id,
            status = "Cancelled",
            cancelledAt = DateTime.UtcNow
        };
    }
}
