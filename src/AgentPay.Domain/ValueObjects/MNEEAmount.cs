using System;

namespace AgentPay.Domain.ValueObjects;

/// <summary>
/// Value object representing MNEE stablecoin amount
/// Ensures decimal precision and validation
/// </summary>
public record MNEEAmount
{
    public decimal Value { get; init; }

    private MNEEAmount(decimal value)
    {
        Value = value;
    }

    public static MNEEAmount Create(decimal value)
    {
        if (value < 0)
            throw new ArgumentException("MNEE amount cannot be negative", nameof(value));

        if (value > 1_000_000_000m)
            throw new ArgumentException("MNEE amount exceeds maximum", nameof(value));

        return new MNEEAmount(value);
    }

    public static MNEEAmount FromDecimal(decimal value) => Create(value);

    public static MNEEAmount Zero => new(0);

    public static MNEEAmount operator +(MNEEAmount a, MNEEAmount b) 
        => Create(a.Value + b.Value);

    public static MNEEAmount operator -(MNEEAmount a, MNEEAmount b) 
        => Create(a.Value - b.Value);

    public static bool operator >(MNEEAmount a, MNEEAmount b) 
        => a.Value > b.Value;

    public static bool operator <(MNEEAmount a, MNEEAmount b) 
        => a.Value < b.Value;

    public override string ToString() => $"{Value:N8} MNEE";
}
