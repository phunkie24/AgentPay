using System;
using System.Collections.Generic;
using System.Linq;
using AgentPay.Domain.ValueObjects;
using AgentPay.Domain.Events;
using AgentPay.Domain.Exceptions;

namespace AgentPay.Domain.Entities;

/// <summary>
/// Core Agent entity representing an AI agent capable of autonomous payments
/// Implements: Multi-Agent Pattern, Role Prompting, Session Memory
/// </summary>
public class Agent
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public AgentRole Role { get; private set; }
    public WalletAddress WalletAddress { get; private set; }
    public AgentStatus Status { get; private set; }
    public AgentCapabilities Capabilities { get; private set; }
    
    // Pattern: Long-Term Memory (Pattern 28)
    public AgentMemory LongTermMemory { get; private set; }
    
    // Pattern: Session Memory (Pattern 26)
    public List<AgentSession> Sessions { get; private set; }
    
    // Pattern: Reflection & Self-Check (Patterns 18, 31)
    public List<AgentReflection> Reflections { get; private set; }
    
    public decimal MNEEBalance { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastActiveAt { get; private set; }
    
    // Domain Events
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Agent() 
    { 
        Sessions = new List<AgentSession>();
        Reflections = new List<AgentReflection>();
    }

    public static Agent Create(
        string name, 
        AgentRole role, 
        WalletAddress walletAddress,
        AgentCapabilities capabilities)
    {
        var agent = new Agent
        {
            Id = Guid.NewGuid(),
            Name = name,
            Role = role,
            WalletAddress = walletAddress,
            Status = AgentStatus.Initializing,
            Capabilities = capabilities,
            LongTermMemory = AgentMemory.CreateEmpty(),
            CreatedAt = DateTime.UtcNow,
            MNEEBalance = 0m
        };

        agent.AddDomainEvent(new AgentCreatedEvent(agent.Id, agent.Name, agent.Role));
        return agent;
    }

    // Pattern: Plan-and-Execute (Pattern 14)
    public AgentPlan CreatePaymentPlan(PaymentGoal goal, decimal budgetLimit)
    {
        if (MNEEBalance < budgetLimit)
            throw new InsufficientBalanceException($"Agent {Name} has insufficient balance");

        var plan = AgentPlan.Create(Id, goal, budgetLimit);
        AddDomainEvent(new PlanCreatedEvent(Id, plan.Id));
        return plan;
    }

    // Pattern: Chain of Thought (Pattern 13)
    public void RecordThoughtProcess(string thought, string reasoning)
    {
        LongTermMemory.AddThought(thought, reasoning, DateTime.UtcNow);
    }

    // Pattern: Session Memory (Pattern 26)
    public AgentSession StartSession(string purpose)
    {
        var session = AgentSession.Create(Id, purpose);
        Sessions.Add(session);
        LastActiveAt = DateTime.UtcNow;
        Status = AgentStatus.Active;
        
        AddDomainEvent(new SessionStartedEvent(Id, session.Id));
        return session;
    }

    // Pattern: Reflection (Pattern 18)
    public void ReflectOnAction(AgentAction action, ActionOutcome outcome)
    {
        var reflection = AgentReflection.Create(
            agentId: Id,
            action: action,
            outcome: outcome,
            timestamp: DateTime.UtcNow
        );
        
        Reflections.Add(reflection);
        LongTermMemory.AddReflection(reflection);
        
        AddDomainEvent(new ReflectionCreatedEvent(Id, reflection.Id));
    }

    // Pattern: Self-Check (Pattern 31)
    public SelfCheckResult PerformSelfCheck()
    {
        var checks = new List<Check>
        {
            CheckBalance(),
            CheckCapabilities(),
            CheckMemoryIntegrity(),
            CheckLastActivity()
        };

        var result = new SelfCheckResult(Id, checks, DateTime.UtcNow);
        
        if (!result.IsHealthy)
        {
            Status = AgentStatus.Degraded;
            AddDomainEvent(new AgentDegradedEvent(Id, result.FailedChecks));
        }

        return result;
    }

    public void UpdateBalance(decimal newBalance)
    {
        var oldBalance = MNEEBalance;
        MNEEBalance = newBalance;
        
        AddDomainEvent(new BalanceUpdatedEvent(Id, oldBalance, newBalance));
    }

    public void Activate()
    {
        Status = AgentStatus.Active;
        LastActiveAt = DateTime.UtcNow;
        AddDomainEvent(new AgentActivatedEvent(Id));
    }

    public void Deactivate()
    {
        Status = AgentStatus.Inactive;
        AddDomainEvent(new AgentDeactivatedEvent(Id));
    }

    private Check CheckBalance() => 
        new Check("Balance", MNEEBalance >= 0, $"Balance: {MNEEBalance}");

    private Check CheckCapabilities() => 
        new Check("Capabilities", Capabilities != null && Capabilities.IsValid(), "Capabilities OK");

    private Check CheckMemoryIntegrity() => 
        new Check("Memory", LongTermMemory != null, "Memory initialized");

    private Check CheckLastActivity() =>
        new Check("Activity", 
            !LastActiveAt.HasValue || (DateTime.UtcNow - LastActiveAt.Value).TotalHours < 24,
            $"Last active: {LastActiveAt?.ToString() ?? "Never"}");

    private void AddDomainEvent(IDomainEvent @event)
    {
        _domainEvents.Add(@event);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

public enum AgentStatus
{
    Initializing,
    Active,
    Inactive,
    Degraded,
    Suspended
}

public enum AgentRole
{
    Planner,        // Pattern: Decomposition, Plan-and-Execute
    Negotiator,     // Pattern: Debate
    Executor,       // Pattern: Tool Calling
    Verifier,       // Pattern: Verification
    Reflector,      // Pattern: Reflection
    Coordinator     // Pattern: Multi-Agent Collaboration
}

// Supporting types for Agent
public record Check(string Name, bool Passed, string Message);
