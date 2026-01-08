using System;

namespace AgentPay.Domain.ValueObjects;

public record WalletAddress
{
    public string Value { get; init; }

    private WalletAddress(string value) => Value = value;

    public static WalletAddress Create(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Wallet address cannot be empty");

        if (!address.StartsWith("0x"))
            throw new ArgumentException("Invalid Ethereum address format");

        if (address.Length != 42)
            throw new ArgumentException("Ethereum address must be 42 characters");

        return new WalletAddress(address);
    }

    public override string ToString() => Value;
}
