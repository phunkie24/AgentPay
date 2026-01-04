using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Nethereum.Web3.Accounts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using AgentPay.Domain.ValueObjects;

namespace AgentPay.Infrastructure.Blockchain;

/// <summary>
/// Wallet management service for creating and managing agent wallets
/// Handles key generation, signing, and address derivation
/// </summary>
public class WalletService
{
    public WalletInfo CreateWallet()
    {
        var ecKey = EthECKey.GenerateKey();
        var privateKey = ecKey.GetPrivateKeyAsBytes().ToHex();
        var address = ecKey.GetPublicAddress();

        return new WalletInfo(
            Address: WalletAddress.Create(address),
            PrivateKey: privateKey,
            PublicKey: ecKey.GetPubKey().ToHex()
        );
    }

    public WalletInfo RestoreWallet(string privateKey)
    {
        var account = new Account(privateKey);

        return new WalletInfo(
            Address: WalletAddress.Create(account.Address),
            PrivateKey: privateKey,
            PublicKey: account.PublicKey
        );
    }

    public bool ValidateAddress(string address)
    {
        try
        {
            WalletAddress.Create(address);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string SignMessage(string message, string privateKey)
    {
        var signer = new EthereumMessageSigner();
        return signer.EncodeUTF8AndSign(message, new EthECKey(privateKey));
    }

    public string RecoverAddressFromSignature(string message, string signature)
    {
        var signer = new EthereumMessageSigner();
        return signer.EncodeUTF8AndEcRecover(message, signature);
    }

    public bool VerifySignature(string message, string signature, string expectedAddress)
    {
        var recoveredAddress = RecoverAddressFromSignature(message, signature);
        return recoveredAddress.Equals(expectedAddress, StringComparison.OrdinalIgnoreCase);
    }
}

public record WalletInfo(
    WalletAddress Address,
    string PrivateKey,
    string PublicKey
);
