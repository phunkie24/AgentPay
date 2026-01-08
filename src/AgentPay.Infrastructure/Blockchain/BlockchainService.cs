using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using AgentPay.Domain.ValueObjects;

namespace AgentPay.Infrastructure.Blockchain;

/// <summary>
/// Blockchain service for interacting with Ethereum-compatible networks
/// Handles MNEE token transfers and balance queries
/// </summary>
public class BlockchainService : IBlockchainService
{
    private readonly Web3 _web3;
    private readonly MNEEContractService _mneeContract;
    private readonly string _rpcUrl;

    public BlockchainService(string rpcUrl, string mneeContractAddress)
    {
        _rpcUrl = rpcUrl;
        _web3 = new Web3(rpcUrl);
        _mneeContract = new MNEEContractService(rpcUrl, mneeContractAddress);
    }

    public async Task<decimal> GetBalanceAsync(WalletAddress address)
    {
        return await _mneeContract.GetBalanceAsync(address.Value);
    }

    public async Task<TransactionHash> TransferAsync(
        WalletAddress from,
        WalletAddress to,
        MNEEAmount amount,
        string privateKey)
    {
        var account = new Account(privateKey);
        var web3WithAccount = new Web3(account, _rpcUrl);

        var txHash = await _mneeContract.TransferAsync(from.Value, to.Value, amount.Value);

        return TransactionHash.Create(txHash);
    }

    public async Task<TransactionReceipt> GetTransactionReceiptAsync(TransactionHash hash)
    {
        var receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(hash.Value);

        if (receipt == null)
        {
            throw new InvalidOperationException($"Transaction receipt not found for hash: {hash.Value}");
        }

        return new TransactionReceipt(
            TransactionHash: receipt.TransactionHash,
            BlockHash: receipt.BlockHash,
            BlockNumber: (long)receipt.BlockNumber.Value,
            From: receipt.From,
            To: receipt.To,
            GasUsed: (decimal)receipt.GasUsed.Value,
            GasPrice: Web3.Convert.FromWei(receipt.EffectiveGasPrice.Value),
            Success: receipt.Status.Value == 1,
            ErrorMessage: receipt.Status.Value == 0 ? "Transaction failed" : null
        );
    }

    public async Task<bool> VerifyTransactionAsync(TransactionHash hash)
    {
        try
        {
            var receipt = await GetTransactionReceiptAsync(hash);
            return receipt.Success;
        }
        catch
        {
            return false;
        }
    }

    public async Task<BlockchainStatus> GetNetworkStatusAsync()
    {
        var latestBlock = await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
        var gasPrice = await _web3.Eth.GasPrice.SendRequestAsync();
        var peerCount = await _web3.Net.PeerCount.SendRequestAsync();
        var isSyncing = await _web3.Eth.Syncing.SendRequestAsync();

        return new BlockchainStatus(
            LatestBlock: (long)latestBlock.Value,
            GasPrice: Web3.Convert.FromWei(gasPrice.Value),
            PeerCount: (int)peerCount.Value,
            IsSyncing: isSyncing.IsSyncing
        );
    }
}
