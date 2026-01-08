using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AgentPay.Domain.ValueObjects;

namespace AgentPay.Infrastructure.Blockchain;

public interface IBlockchainService
{
    Task<decimal> GetBalanceAsync(WalletAddress address);
    Task<TransactionHash> TransferAsync(WalletAddress from, WalletAddress to, MNEEAmount amount, string privateKey);
    Task<TransactionReceipt> GetTransactionReceiptAsync(TransactionHash hash);
    Task<bool> VerifyTransactionAsync(TransactionHash hash);
    Task<BlockchainStatus> GetNetworkStatusAsync();
}

public record TransactionReceipt(
    string TransactionHash,
    string BlockHash,
    long BlockNumber,
    string From,
    string To,
    decimal GasUsed,
    decimal GasPrice,
    bool Success,
    string? ErrorMessage = null
);

public record BlockchainStatus(
    long LatestBlock,
    decimal GasPrice,
    int PeerCount,
    bool IsSyncing
);
