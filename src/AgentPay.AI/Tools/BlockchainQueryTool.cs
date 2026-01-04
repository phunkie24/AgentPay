using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// using AgentPay.Infrastructure.Blockchain; // Commented to avoid circular dependency

namespace AgentPay.AI.Tools;

/// <summary>
/// Tool for querying blockchain data
/// </summary>
public class BlockchainQueryTool : ITool
{
    private readonly dynamic _contractService; // TODO: Replace with IBlockchainService interface

    public string Name => "blockchain_query";
    public string Description => "Query MNEE token balances, transactions, and blockchain data";

    public BlockchainQueryTool(dynamic contractService) // TODO: Inject proper abstraction
    {
        _contractService = contractService;
    }

    public async Task<object> ExecuteAsync(Dictionary<string, object> parameters)
    {
        var action = parameters.GetValueOrDefault("action")?.ToString();

        return action switch
        {
            "get_balance" => await GetBalanceAsync(parameters),
            "get_transaction" => await GetTransactionAsync(parameters),
            "verify_payment" => await VerifyPaymentAsync(parameters),
            _ => throw new ArgumentException($"Unknown action: {action}")
        };
    }

    private async Task<object> GetBalanceAsync(Dictionary<string, object> parameters)
    {
        var address = parameters.GetValueOrDefault("address")?.ToString();
        if (string.IsNullOrEmpty(address))
            throw new ArgumentException("Address required");

        var balance = await _contractService.GetBalanceAsync(address);
        return new { address, balance = balance.ToString() };
    }

    private async Task<object> GetTransactionAsync(Dictionary<string, object> parameters)
    {
        var txHash = parameters.GetValueOrDefault("transaction_hash")?.ToString();
        if (string.IsNullOrEmpty(txHash))
            throw new ArgumentException("Transaction hash required");

        var receipt = await _contractService.GetTransactionReceiptAsync(txHash);
        return new
        {
            transactionHash = txHash,
            status = receipt != null ? "confirmed" : "pending",
            blockNumber = receipt?.BlockNumber.ToString()
        };
    }

    private async Task<object> VerifyPaymentAsync(Dictionary<string, object> parameters)
    {
        var txHash = parameters.GetValueOrDefault("transaction_hash")?.ToString();
        var expectedAmount = parameters.GetValueOrDefault("expected_amount")?.ToString();

        var isValid = await _contractService.VerifyPaymentAsync(txHash, decimal.Parse(expectedAmount ?? "0"));

        return new
        {
            transactionHash = txHash,
            isValid,
            verified = DateTime.UtcNow
        };
    }
}
