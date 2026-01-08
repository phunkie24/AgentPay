using System;
using System.Collections.Generic;
using AgentPay.Domain.ValueObjects;

namespace AgentPay.Domain.Entities;

public class PaymentSession
{
    public Guid Id { get; private set; }
    public Guid AgentId { get; private set; }
    public Guid ServiceId { get; private set; }
    public PaymentSessionStatus Status { get; private set; }
    public MNEEAmount BudgetLimit { get; private set; }
    public MNEEAmount? NegotiatedPrice { get; private set; }
    public List<PaymentStep> Steps { get; private set; } = new();
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private PaymentSession() { }

    public static PaymentSession Start(Guid agentId, Guid serviceId, MNEEAmount budgetLimit)
    {
        return new PaymentSession
        {
            Id = Guid.NewGuid(),
            AgentId = agentId,
            ServiceId = serviceId,
            BudgetLimit = budgetLimit,
            Status = PaymentSessionStatus.Planning,
            StartedAt = DateTime.UtcNow
        };
    }

    public void AddStep(PaymentStepType type, string description, object data)
    {
        Steps.Add(new PaymentStep
        {
            Type = type,
            Description = description,
            Data = System.Text.Json.JsonSerializer.Serialize(data),
            Timestamp = DateTime.UtcNow
        });
    }

    public void SetNegotiatedPrice(MNEEAmount price)
    {
        NegotiatedPrice = price;
        Status = PaymentSessionStatus.Negotiated;
    }

    public void Complete()
    {
        Status = PaymentSessionStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Fail(string reason)
    {
        Status = PaymentSessionStatus.Failed;
        AddStep(PaymentStepType.Error, "Session failed", new { Reason = reason });
        CompletedAt = DateTime.UtcNow;
    }
}

public enum PaymentSessionStatus
{
    Planning,
    Discovering,
    Negotiating,
    Negotiated,
    Executing,
    Verifying,
    Completed,
    Failed
}

public enum PaymentStepType
{
    Planning,
    Discovery,
    Negotiation,
    Guardrails,
    Execution,
    Verification,
    Reflection,
    Error
}

public class PaymentStep
{
    public PaymentStepType Type { get; set; }
    public string Description { get; set; }
    public string Data { get; set; }
    public DateTime Timestamp { get; set; }
}
