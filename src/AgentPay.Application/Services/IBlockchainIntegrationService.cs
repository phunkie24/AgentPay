using System;
using System.Threading.Tasks;
using AgentPay.Application.DTOs;

namespace AgentPay.Application.Services;

/// <summary>
/// High-level blockchain integration for application layer
/// </summary>
public interface IBlockchainIntegrationService
{
    Task<decimal> GetAgentBalanceAsync(Guid agentId);
    Task<string> ExecuteTransactionAsync(Guid transactionId);
    Task<TransactionReceiptDto> GetTransactionReceiptAsync(string transactionHash);
    Task<BlockchainStatusDto> GetNetworkStatusAsync();
    Task<bool> VerifyTransactionAsync(Guid transactionId);
}
