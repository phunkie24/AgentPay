using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using AgentPay.Domain.ValueObjects;

namespace AgentPay.Infrastructure.Blockchain;

public class MNEEContractService
{
    private readonly Web3 _web3;
    private readonly string _contractAddress;
    private readonly Contract _contract;

    public MNEEContractService(string rpcUrl, string contractAddress)
    {
        _web3 = new Web3(rpcUrl);
        _contractAddress = contractAddress;
        
        // MNEE ERC20 ABI
        var abi = @"[{""constant"":true,""inputs"":[{""name"":""_owner"",""type"":""address""}],""name"":""balanceOf"",""outputs"":[{""name"":""balance"",""type"":""uint256""}],""type"":""function""},{""constant"":false,""inputs"":[{""name"":""_to"",""type"":""address""},{""name"":""_value"",""type"":""uint256""}],""name"":""transfer"",""outputs"":[{""name"":"""",""type"":""bool""}],""type"":""function""}]";
        
        _contract = _web3.Eth.GetContract(abi, _contractAddress);
    }

    public async Task<decimal> GetBalanceAsync(string address)
    {
        var balanceFunction = _contract.GetFunction("balanceOf");
        var balance = await balanceFunction.CallAsync<BigInteger>(address);
        return Web3.Convert.FromWei(balance);
    }

    public async Task<string> TransferAsync(string fromAddress, string toAddress, decimal amount)
    {
        var transferFunction = _contract.GetFunction("transfer");
        var amountInWei = Web3.Convert.ToWei(amount);
        
        var receipt = await transferFunction.SendTransactionAndWaitForReceiptAsync(
            fromAddress,
            new HexBigInteger(300000),
            new HexBigInteger(0),
            null,
            toAddress,
            amountInWei);
        
        return receipt.TransactionHash;
    }
}
