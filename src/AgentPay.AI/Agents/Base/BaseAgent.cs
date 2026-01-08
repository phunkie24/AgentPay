using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Tools;
using AgentPay.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace AgentPay.AI.Agents.Base;

public abstract class BaseAgent : IAgent
{
    protected readonly IToolRegistry _toolRegistry;
    protected readonly ILogger _logger;

    public string Name { get; protected set; }
    public AgentRole Role { get; protected set; }

    protected BaseAgent(IToolRegistry toolRegistry, ILogger logger)
    {
        _toolRegistry = toolRegistry;
        _logger = logger;
    }

    public abstract Task<AgentResult> ExecuteAsync(AgentTask task, AgentContext context);

    public abstract Task<ReflectionResult> ReflectOnResultAsync(AgentResult result);

    public virtual async Task<HealthCheck> PerformHealthCheckAsync()
    {
        return new HealthCheck
        {
            IsHealthy = true,
            Issues = new List<string>(),
            CheckedAt = DateTime.UtcNow
        };
    }

    protected void LogInfo(string message)
    {
        _logger.LogInformation($"[{Name}] {message}");
    }

    protected void LogError(string message, Exception ex = null)
    {
        _logger.LogError(ex, $"[{Name}] {message}");
    }
}
