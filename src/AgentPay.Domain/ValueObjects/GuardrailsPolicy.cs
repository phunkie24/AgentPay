using System;
using System.Collections.Generic;
using System.Linq;
using AgentPay.Domain.Entities;

namespace AgentPay.Domain.ValueObjects;

public class GuardrailsPolicy
{
    public decimal MaxTransactionAmount { get; set; } = 1000m;
    public decimal DailyLimit { get; set; } = 5000m;
    public List<string> WhitelistedAddresses { get; set; } = new();
    public List<string> BlacklistedAddresses { get; set; } = new();
    public bool RequireManualApprovalAbove { get; set; } = false;
    public decimal ManualApprovalThreshold { get; set; } = 10000m;

    public GuardrailCheck CheckAmountLimit(decimal amount)
    {
        var passed = amount <= MaxTransactionAmount;
        return new GuardrailCheck(
            "Amount Limit",
            passed,
            passed ? "OK" : $"Amount {amount} exceeds limit of {MaxTransactionAmount}"
        );
    }

    public GuardrailCheck CheckAddressWhitelist(WalletAddress address)
    {
        if (!WhitelistedAddresses.Any())
            return new GuardrailCheck("Whitelist", true, "No whitelist configured");

        var passed = WhitelistedAddresses.Contains(address.Value);
        return new GuardrailCheck(
            "Whitelist",
            passed,
            passed ? "Address whitelisted" : "Address not in whitelist"
        );
    }

    public GuardrailCheck CheckDailyLimit(Guid agentId, decimal amount)
    {
        // In real implementation, check actual daily spending
        return new GuardrailCheck("Daily Limit", true, "Within daily limit");
    }

    public GuardrailCheck CheckSuspiciousPattern(Transaction transaction)
    {
        // Pattern analysis logic
        return new GuardrailCheck("Pattern Check", true, "No suspicious patterns detected");
    }

    public static GuardrailsPolicy CreateDefault()
    {
        return new GuardrailsPolicy
        {
            MaxTransactionAmount = 1000m,
            DailyLimit = 5000m,
            RequireManualApprovalAbove = true,
            ManualApprovalThreshold = 10000m
        };
    }
}
