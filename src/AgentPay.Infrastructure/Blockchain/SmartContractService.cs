using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;

namespace AgentPay.Infrastructure.Blockchain;

/// <summary>
/// Generic smart contract interaction service
/// Supports deployment, function calls, and event monitoring
/// </summary>
public class SmartContractService
{
    private readonly Web3 _web3;

    public SmartContractService(string rpcUrl)
    {
        _web3 = new Web3(rpcUrl);
    }

    public async Task<string> DeployContractAsync(
        string abi,
        string bytecode,
        string deployerAddress,
        params object[] constructorParams)
    {
        // Deploy contract using raw transaction approach
        var transactionInput = new Nethereum.RPC.Eth.DTOs.TransactionInput
        {
            From = deployerAddress,
            Data = bytecode,
            Gas = new HexBigInteger(3000000)
        };

        var txHash = await _web3.Eth.Transactions.SendTransaction.SendRequestAsync(transactionInput);
        var receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash);

        // Wait for confirmation
        while (receipt == null)
        {
            await Task.Delay(1000);
            receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash);
        }

        return receipt.ContractAddress;
    }

    public async Task<T> CallFunctionAsync<T>(
        string contractAddress,
        string abi,
        string functionName,
        params object[] parameters)
    {
        var contract = _web3.Eth.GetContract(abi, contractAddress);
        var function = contract.GetFunction(functionName);
        return await function.CallAsync<T>(parameters);
    }

    public async Task<string> SendTransactionAsync(
        string contractAddress,
        string abi,
        string functionName,
        string fromAddress,
        params object[] parameters)
    {
        var contract = _web3.Eth.GetContract(abi, contractAddress);
        var function = contract.GetFunction(functionName);

        var receipt = await function.SendTransactionAndWaitForReceiptAsync(
            fromAddress,
            new HexBigInteger(300000),
            new HexBigInteger(0),
            null,
            parameters);

        return receipt.TransactionHash;
    }

    public async Task<List<TEvent>> GetEventsAsync<TEvent>(
        string contractAddress,
        string abi,
        string eventName,
        long fromBlock,
        long? toBlock = null) where TEvent : class, new()
    {
        var contract = _web3.Eth.GetContract(abi, contractAddress);
        var eventHandler = contract.GetEvent(eventName);

        var filterInput = eventHandler.CreateFilterInput(
            new BlockParameter((ulong)fromBlock),
            toBlock.HasValue ? new BlockParameter((ulong)toBlock.Value) : BlockParameter.CreateLatest());

        var events = await eventHandler.GetAllChangesAsync<TEvent>(filterInput);
        return events.Select(e => e.Event).ToList();
    }
}
