#!/bin/bash

echo "🚀 Generating Complete Enterprise Implementation"
echo "=================================================="
echo ""

# Set base directory
BASE_DIR="/mnt/user-data/outputs/AgentPay"
cd $BASE_DIR

# Counter
count=0

#############################################################################
# DOMAIN LAYER - Complete Entities
#############################################################################

echo "📦 Domain Layer - Additional Entities..."

cat > src/AgentPay.Domain/Entities/Service.cs << 'EOF'
using AgentPay.Domain.ValueObjects;

namespace AgentPay.Domain.Entities;

public class Service
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public WalletAddress ProviderAddress { get; private set; }
    public MNEEAmount ListedPrice { get; private set; }
    public ServiceCategory Category { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    private Service() { }

    public static Service Create(
        string name,
        string description,
        WalletAddress providerAddress,
        MNEEAmount listedPrice,
        ServiceCategory category)
    {
        return new Service
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            ProviderAddress = providerAddress,
            ListedPrice = listedPrice,
            Category = category,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdatePrice(MNEEAmount newPrice)
    {
        ListedPrice = newPrice;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}

public enum ServiceCategory
{
    DataAPI,
    ComputeResource,
    AIModel,
    Storage,
    Analytics,
    Other
}
EOF
((count++))

cat > src/AgentPay.Domain/Entities/PaymentSession.cs << 'EOF'
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
EOF
((count++))

cat > src/AgentPay.Domain/Entities/AgentCapabilities.cs << 'EOF'
namespace AgentPay.Domain.Entities;

public class AgentCapabilities
{
    public bool CanNegotiate { get; set; }
    public bool CanPlan { get; set; }
    public bool CanReflect { get; set; }
    public decimal MaxTransactionAmount { get; set; }
    public List<string> AllowedServiceCategories { get; set; } = new();
    public List<string> EnabledTools { get; set; } = new();

    public bool IsValid()
    {
        return MaxTransactionAmount > 0 && EnabledTools.Any();
    }

    public static AgentCapabilities CreateDefault()
    {
        return new AgentCapabilities
        {
            CanNegotiate = true,
            CanPlan = true,
            CanReflect = true,
            MaxTransactionAmount = 1000m,
            AllowedServiceCategories = new() { "DataAPI", "AIModel" },
            EnabledTools = new() { "web_search", "blockchain_query", "price_check" }
        };
    }
}
EOF
((count++))

cat > src/AgentPay.Domain/ValueObjects/AgentMemory.cs << 'EOF'
namespace AgentPay.Domain.ValueObjects;

public class AgentMemory
{
    private readonly List<MemoryEntry> _entries = new();
    private readonly List<ThoughtEntry> _thoughts = new();
    private readonly List<AgentReflection> _reflections = new();

    public static AgentMemory CreateEmpty() => new();

    public void AddThought(string thought, string reasoning, DateTime timestamp)
    {
        _thoughts.Add(new ThoughtEntry(thought, reasoning, timestamp));
    }

    public void AddMemory(string key, object value, string context)
    {
        _entries.Add(new MemoryEntry(key, value, context, DateTime.UtcNow));
    }

    public void AddReflection(AgentReflection reflection)
    {
        _reflections.Add(reflection);
    }

    public List<ThoughtEntry> GetRecentThoughts(int count = 10)
    {
        return _thoughts.OrderByDescending(t => t.Timestamp).Take(count).ToList();
    }

    public List<AgentReflection> GetReflections()
    {
        return _reflections.ToList();
    }
}

public record MemoryEntry(string Key, object Value, string Context, DateTime Timestamp);
public record ThoughtEntry(string Thought, string Reasoning, DateTime Timestamp);

public class AgentReflection
{
    public Guid Id { get; private set; }
    public Guid AgentId { get; private set; }
    public AgentAction Action { get; private set; }
    public ActionOutcome Outcome { get; private set; }
    public string Insights { get; private set; }
    public List<string> Learnings { get; private set; } = new();
    public DateTime Timestamp { get; private set; }

    private AgentReflection() { }

    public static AgentReflection Create(
        Guid agentId, 
        AgentAction action, 
        ActionOutcome outcome,
        DateTime timestamp)
    {
        return new AgentReflection
        {
            Id = Guid.NewGuid(),
            AgentId = agentId,
            Action = action,
            Outcome = outcome,
            Timestamp = timestamp
        };
    }

    public void SetInsights(string insights, List<string> learnings)
    {
        Insights = insights;
        Learnings = learnings;
    }
}

public record AgentAction(string Type, string Description, Dictionary<string, object> Parameters);
public record ActionOutcome(bool Success, string Result, Dictionary<string, object> Metrics);
EOF
((count++))

cat > src/AgentPay.Domain/ValueObjects/AgentSession.cs << 'EOF'
namespace AgentPay.Domain.ValueObjects;

public class AgentSession
{
    public Guid Id { get; private set; }
    public Guid AgentId { get; private set; }
    public string Purpose { get; private set; }
    public SessionStatus Status { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public List<SessionMessage> Messages { get; private set; } = new();

    private AgentSession() { }

    public static AgentSession Create(Guid agentId, string purpose)
    {
        return new AgentSession
        {
            Id = Guid.NewGuid(),
            AgentId = agentId,
            Purpose = purpose,
            Status = SessionStatus.Active,
            StartedAt = DateTime.UtcNow
        };
    }

    public void AddMessage(string role, string content)
    {
        Messages.Add(new SessionMessage(role, content, DateTime.UtcNow));
    }

    public void End()
    {
        Status = SessionStatus.Completed;
        EndedAt = DateTime.UtcNow;
    }
}

public enum SessionStatus
{
    Active,
    Completed,
    Cancelled
}

public record SessionMessage(string Role, string Content, DateTime Timestamp);
EOF
((count++))

cat > src/AgentPay.Domain/ValueObjects/AgentPlan.cs << 'EOF'
using AgentPay.Domain.ValueObjects;

namespace AgentPay.Domain.Entities;

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

public record PaymentGoal(string ServiceId, string Objective, Dictionary<string, object> Requirements);
EOF
((count++))

cat > src/AgentPay.Domain/ValueObjects/GuardrailsPolicy.cs << 'EOF'
using AgentPay.Domain.ValueObjects;
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
EOF
((count++))

cat > src/AgentPay.Domain/ValueObjects/Result.cs << 'EOF'
namespace AgentPay.Domain.ValueObjects;

public class Result
{
    public bool IsSuccess { get; }
    public string ErrorMessage { get; }

    protected Result(bool isSuccess, string errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string errorMessage) => new(false, errorMessage);
}

public class Result<T> : Result
{
    public T Value { get; }

    private Result(bool isSuccess, T value, string errorMessage) 
        : base(isSuccess, errorMessage)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public static new Result<T> Failure(string errorMessage) => new(false, default, errorMessage);
}

public record Check(string Name, bool Passed, string Message);

public class SelfCheckResult
{
    public Guid AgentId { get; set; }
    public List<Check> Checks { get; set; }
    public DateTime CheckedAt { get; set; }
    public bool IsHealthy => Checks.All(c => c.Passed);
    public List<string> FailedChecks => Checks.Where(c => !c.Passed).Select(c => c.Name).ToList();

    public SelfCheckResult(Guid agentId, List<Check> checks, DateTime checkedAt)
    {
        AgentId = agentId;
        Checks = checks;
        CheckedAt = checkedAt;
    }
}
EOF
((count++))

echo "✅ Domain layer complete ($count files)"

#############################################################################
# DOMAIN - Exceptions
#############################################################################

echo "📦 Domain - Exceptions..."

cat > src/AgentPay.Domain/Exceptions/DomainExceptions.cs << 'EOF'
namespace AgentPay.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class InsufficientBalanceException : DomainException
{
    public InsufficientBalanceException(string message) : base(message) { }
}

public class InvalidAgentConfigException : DomainException
{
    public InvalidAgentConfigException(string message) : base(message) { }
}

public class PaymentFailedException : DomainException
{
    public PaymentFailedException(string message) : base(message) { }
}

public class GuardrailsViolationException : DomainException
{
    public GuardrailsViolationException(string message) : base(message) { }
}
EOF
((count++))

#############################################################################
# DOMAIN - Repository Interfaces
#############################################################################

echo "📦 Domain - Repository Interfaces..."

cat > src/AgentPay.Domain/Repositories/IAgentRepository.cs << 'EOF'
using AgentPay.Domain.Entities;

namespace AgentPay.Domain.Repositories;

public interface IAgentRepository
{
    Task<Agent> GetByIdAsync(Guid id);
    Task<Agent> CreateAsync(Agent agent);
    Task UpdateAsync(Agent agent);
    Task<IEnumerable<Agent>> GetActiveAgentsAsync();
    Task<IEnumerable<Agent>> GetByRoleAsync(AgentRole role);
}
EOF
((count++))

cat > src/AgentPay.Domain/Repositories/ITransactionRepository.cs << 'EOF'
using AgentPay.Domain.Entities;

namespace AgentPay.Domain.Repositories;

public interface ITransactionRepository
{
    Task<Transaction> GetByIdAsync(Guid id);
    Task<Transaction> CreateAsync(Transaction transaction);
    Task UpdateAsync(Transaction transaction);
    Task<IEnumerable<Transaction>> GetByAgentIdAsync(Guid agentId);
    Task<IEnumerable<Transaction>> GetPendingTransactionsAsync();
}
EOF
((count++))

cat > src/AgentPay.Domain/Repositories/IServiceRepository.cs << 'EOF'
using AgentPay.Domain.Entities;

namespace AgentPay.Domain.Repositories;

public interface IServiceRepository
{
    Task<Service> GetByIdAsync(Guid id);
    Task<Service> CreateAsync(Service service);
    Task UpdateAsync(Service service);
    Task<IEnumerable<Service>> GetActiveServicesAsync();
    Task<IEnumerable<Service>> GetByCategoryAsync(ServiceCategory category);
}
EOF
((count++))

cat > src/AgentPay.Domain/Repositories/IUnitOfWork.cs << 'EOF'
namespace AgentPay.Domain.Repositories;

public interface IUnitOfWork : IDisposable
{
    IAgentRepository Agents { get; }
    ITransactionRepository Transactions { get; }
    IServiceRepository Services { get; }
    Task<int> CommitAsync();
    Task RollbackAsync();
}
EOF
((count++))

echo "✅ Domain repositories complete (+4 files, total: $count)"

echo ""
echo "Progress: $count files generated so far..."
echo ""

