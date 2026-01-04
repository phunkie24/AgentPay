using System;
using System.Collections.Generic;
using System.Linq;
using AgentPay.Domain.Entities;

namespace AgentPay.Domain.ValueObjects;

public class AgentPlan
{
    public Guid Id { get; private set; }
    public Guid AgentId { get; private set; }
    public PaymentGoal Goal { get; private set; }
    public decimal BudgetLimit { get; private set; }
    public List<PlanStep> Steps { get; private set; } = new();
    public PlanStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private AgentPlan() { }

    public static AgentPlan Create(Guid agentId, PaymentGoal goal, decimal budgetLimit)
    {
        return new AgentPlan
        {
            Id = Guid.NewGuid(),
            AgentId = agentId,
            Goal = goal,
            BudgetLimit = budgetLimit,
            Status = PlanStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AddStep(string description, int order, bool isCritical)
    {
        Steps.Add(new PlanStep
        {
            Description = description,
            Order = order,
            IsCritical = isCritical,
            Status = StepStatus.Pending
        });
    }

    public void Approve() => Status = PlanStatus.Approved;
    public void Execute() => Status = PlanStatus.Executing;
    public void Complete() => Status = PlanStatus.Completed;
}

public enum PlanStatus
{
    Draft,
    Approved,
    Executing,
    Completed,
    Failed
}

public enum StepStatus
{
    Pending,
    InProgress,
    Completed,
    Failed,
    Skipped
}

public class PlanStep
{
    public string Description { get; set; }
    public int Order { get; set; }
    public bool IsCritical { get; set; }
    public StepStatus Status { get; set; }
}

public record PaymentGoal(string Description, Guid ServiceId, decimal MaxBudget);

public record SelfCheckResult(Guid AgentId, List<Check> Checks, DateTime PerformedAt)
{
    public bool IsHealthy => Checks.All(c => c.Passed);
    public List<string> FailedChecks => Checks.Where(c => !c.Passed).Select(c => c.Name).ToList();
}
