using System;

namespace AgentPay.Domain.ValueObjects;

public record TransactionHash
{
    public string Value { get; init; }

    private TransactionHash(string value) => Value = value;

    public static TransactionHash Create(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new ArgumentException("Transaction hash cannot be empty");

        if (!hash.StartsWith("0x"))
            throw new ArgumentException("Invalid transaction hash format");

        return new TransactionHash(hash);
    }

    public override string ToString() => Value;
}
