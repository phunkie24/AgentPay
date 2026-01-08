using System;
using System.Collections.Generic;
using System.Linq;
using AgentPay.Domain.ValueObjects;
using AgentPay.Domain.Events;

namespace AgentPay.Domain.Entities;

/// <summary>
/// Transaction entity representing a payment made by an agent
/// Implements: Verification Pattern, Guardrails Pattern
/// </summary>
public class Transaction
{
    public Guid Id { get; private set; }
    public Guid AgentId { get; private set; }
    public Guid ServiceId { get; private set; }
    
    public MNEEAmount Amount { get; private set; }
    public TransactionHash Hash { get; private set; }
    public TransactionStatus Status { get; private set; }
    
    public WalletAddress FromAddress { get; private set; }
    public WalletAddress ToAddress { get; private set; }
    
    // Pattern: Verification (Pattern 19)
    public VerificationResult VerificationResult { get; private set; }
    
    // Pattern: Chain of Thought - Reasoning for transaction
    public string Reasoning { get; private set; }
    
    // Pattern: Guardrails (Pattern 32)
    public GuardrailsCheck GuardrailsCheck { get; private set; }
    
    public DateTime InitiatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? FailedAt { get; private set; }
    
    public string? FailureReason { get; private set; }
    public int GasUsed { get; private set; }
    public decimal GasPriceGwei { get; private set; }
    
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Transaction() { }

    public static Transaction Initiate(
        Guid agentId,
        Guid serviceId,
        MNEEAmount amount,
        WalletAddress fromAddress,
        WalletAddress toAddress,
        string reasoning)
    {
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AgentId = agentId,
            ServiceId = serviceId,
            Amount = amount,
            FromAddress = fromAddress,
            ToAddress = toAddress,
            Reasoning = reasoning,
            Status = TransactionStatus.Pending,
            InitiatedAt = DateTime.UtcNow
        };

        transaction.AddDomainEvent(new TransactionInitiatedEvent(
            transaction.Id, 
            transaction.AgentId, 
            transaction.Amount.Value));

        return transaction;
    }

    // Pattern: Guardrails (Pattern 32) - Safety checks before execution
    public Result ApplyGuardrails(GuardrailsPolicy policy)
    {
        var checks = new List<GuardrailCheck>
        {
            policy.CheckAmountLimit(Amount.Value),
            policy.CheckAddressWhitelist(ToAddress),
            policy.CheckDailyLimit(AgentId, Amount.Value),
            policy.CheckSuspiciousPattern(this)
        };

        GuardrailsCheck = new GuardrailsCheck(checks, DateTime.UtcNow);

        if (!GuardrailsCheck.Passed)
        {
            Status = TransactionStatus.Blocked;
            FailureReason = GuardrailsCheck.FailureReason;
            FailedAt = DateTime.UtcNow;
            
            AddDomainEvent(new TransactionBlockedEvent(Id, FailureReason));
            return Result.Failure(FailureReason);
        }

        return Result.Success();
    }

    public void MarkAsPending(TransactionHash hash)
    {
        Hash = hash;
        Status = TransactionStatus.Pending;
        AddDomainEvent(new TransactionSubmittedEvent(Id, Hash.Value));
    }

    // Pattern: Verification (Pattern 19)
    public void Complete(VerificationResult verificationResult, int gasUsed, decimal gasPriceGwei)
    {
        if (Status != TransactionStatus.Pending)
            throw new InvalidOperationException("Only pending transactions can be completed");

        VerificationResult = verificationResult;
        GasUsed = gasUsed;
        GasPriceGwei = gasPriceGwei;
        
        if (verificationResult.IsVerified)
        {
            Status = TransactionStatus.Completed;
            CompletedAt = DateTime.UtcNow;
            AddDomainEvent(new TransactionCompletedEvent(Id, Hash.Value, Amount.Value));
        }
        else
        {
            Status = TransactionStatus.Failed;
            FailedAt = DateTime.UtcNow;
            FailureReason = verificationResult.FailureReason;
            AddDomainEvent(new TransactionFailedEvent(Id, FailureReason));
        }
    }

    public void Fail(string reason)
    {
        Status = TransactionStatus.Failed;
        FailedAt = DateTime.UtcNow;
        FailureReason = reason;
        AddDomainEvent(new TransactionFailedEvent(Id, reason));
    }

    private void AddDomainEvent(IDomainEvent @event)
    {
        _domainEvents.Add(@event);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

public enum TransactionStatus
{
    Pending,
    Submitted,
    Completed,
    Failed,
    Blocked
}

// Pattern: Verification Result
public record VerificationResult(
    bool IsVerified,
    string? FailureReason = null,
    Dictionary<string, object>? Metadata = null);

// Pattern: Guardrails Check Result
public record GuardrailsCheck(
    List<GuardrailCheck> Checks,
    DateTime CheckedAt)
{
    public bool Passed => Checks.All(c => c.Passed);
    public string FailureReason => string.Join("; ", Checks.Where(c => !c.Passed).Select(c => c.Reason));
}

public record GuardrailCheck(
    string Name,
    bool Passed,
    string Reason);
