using System;
using AgentPay.Domain.Entities;

namespace AgentPay.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}

public record AgentCreatedEvent(Guid AgentId, string Name, AgentRole Role) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record PaymentInitiatedEvent(Guid TransactionId, Guid AgentId, decimal Amount) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record PaymentCompletedEvent(Guid TransactionId, string TransactionHash, decimal Amount) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record PaymentFailedEvent(Guid TransactionId, string Reason) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record TransactionBlockedEvent(Guid TransactionId, string Reason) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record AgentActivatedEvent(Guid AgentId) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record AgentDeactivatedEvent(Guid AgentId) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record BalanceUpdatedEvent(Guid AgentId, decimal OldBalance, decimal NewBalance) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record ReflectionCreatedEvent(Guid AgentId, Guid ReflectionId) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record PlanCreatedEvent(Guid AgentId, Guid PlanId) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record SessionStartedEvent(Guid AgentId, Guid SessionId) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record AgentDegradedEvent(Guid AgentId, System.Collections.Generic.List<string> FailedChecks) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record TransactionInitiatedEvent(Guid TransactionId, Guid AgentId, decimal Amount) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record TransactionSubmittedEvent(Guid TransactionId, string TransactionHash) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record TransactionCompletedEvent(Guid TransactionId, string TransactionHash, decimal Amount) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record TransactionFailedEvent(Guid TransactionId, string Reason) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
