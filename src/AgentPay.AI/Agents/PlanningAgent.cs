using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Tools;
using AgentPay.AI.Agents.Base;
using AgentPay.AI.Services;
using AgentPay.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AgentPay.AI.Agents;

/// <summary>
/// Planning Agent: Creates strategic plans and decomposes complex tasks
/// Implements: Pattern 12 (Decomposition), Pattern 14 (Plan-and-Execute), Pattern 11 (Step-Back Prompting)
/// </summary>
public class PlanningAgent : BaseAgent
{
    private readonly ILLMService _llm;
    private readonly ILogger<PlanningAgent> _logger;

    public PlanningAgent(
        ILLMService llm,
        IToolRegistry toolRegistry,
        ILogger<PlanningAgent> logger) 
        : base(toolRegistry, logger)
    {
        _llm = llm;
        _logger = logger;
        Name = "Planning Agent";
        Role = AgentRole.Planner;
    }

    public override async Task<AgentResult> ExecuteAsync(AgentTask task, AgentContext context)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            // Pattern: Step-Back Prompting (Pattern 11) - Think at higher level first
            var highLevelStrategy = await GenerateStrategyAsync(task, context);
            context.AddMessage("assistant", $"Strategy: {highLevelStrategy}");

            // Pattern: Decomposition (Pattern 12) - Break into subtasks
            var subtasks = await DecomposeTaskAsync(task, highLevelStrategy, context);
            context.AddMessage("assistant", $"Subtasks: {JsonSerializer.Serialize(subtasks)}");

            // Pattern: Plan-and-Execute (Pattern 14) - Create execution plan
            var executionPlan = await CreateExecutionPlanAsync(subtasks, context);
            
            // Execute the plan step by step
            var results = new List<SubtaskResult>();
            foreach (var step in executionPlan.Steps)
            {
                var result = await ExecuteStepAsync(step, context);
                results.Add(result);
                
                // If a step fails and is critical, stop execution
                if (!result.Success && step.IsCritical)
                {
                    _logger.LogWarning($"Critical step failed: {step.Description}");
                    break;
                }
            }

            // Synthesize final result
            var finalOutput = await SynthesizeResultsAsync(task, results, context);

            return new AgentResult
            {
                Success = results.All(r => !r.IsCritical || r.Success),
                Output = finalOutput,
                Reasoning = $"Strategy: {highLevelStrategy}\nPlan: {JsonSerializer.Serialize(executionPlan)}",
                ExecutionTime = DateTime.UtcNow - startTime,
                Metadata = new Dictionary<string, object>
                {
                    ["subtasks_count"] = subtasks.Count,
                    ["completed_steps"] = results.Count(r => r.Success),
                    ["failed_steps"] = results.Count(r => !r.Success)
                },
                ConfidenceScore = CalculatePlanConfidence(results)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Planning agent execution failed");
            return new AgentResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ExecutionTime = DateTime.UtcNow - startTime
            };
        }
    }

    // Pattern: Step-Back Prompting (Pattern 11)
    private async Task<string> GenerateStrategyAsync(AgentTask task, AgentContext context)
    {
        var prompt = $$"""
        Before diving into details, let's think about the bigger picture.
        
        Task: {{task.Objective}}
        
        Step back and consider:
        1. What is the fundamental goal here?
        2. What are the key constraints and requirements?
        3. What is the best high-level approach?
        4. What are potential risks or failure modes?
        
        Provide a strategic overview (2-3 sentences):
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.7,
            MaxTokens = 300,
            SystemPrompt = "You are a strategic planning expert. Think at a high level before diving into details."
        });

        return response.Text;
    }

    // Pattern: Decomposition (Pattern 12)
    private async Task<List<Subtask>> DecomposeTaskAsync(
        AgentTask task, 
        string strategy, 
        AgentContext context)
    {
        var prompt = $$"""
        Given the task and strategy:
        
        Task: {{task.Objective}}
        Strategy: {{strategy}}
        
        Break this down into concrete, actionable subtasks. Each subtask should be:
        - Specific and measurable
        - Executable with available tools
        - Properly sequenced
        
        Respond in JSON format:
        {
            "subtasks": [
                {
                    "id": 1,
                    "description": "...",
                    "dependencies": [],
                    "estimatedDuration": "5m",
                    "isCritical": true,
                    "requiredTools": ["tool1"]
                }
            ]
        }
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.5,
            ResponseFormat = "json",
            MaxTokens = 1500
        });

        var decomposition = JsonSerializer.Deserialize<DecompositionResult>(response.Text);
        return decomposition.Subtasks;
    }

    // Pattern: Plan-and-Execute (Pattern 14)
    private async Task<ExecutionPlan> CreateExecutionPlanAsync(
        List<Subtask> subtasks, 
        AgentContext context)
    {
        // Perform topological sort based on dependencies
        var sortedTasks = TopologicalSort(subtasks);
        
        var steps = sortedTasks.Select((task, index) => new ExecutionStep
        {
            Order = index + 1,
            SubtaskId = task.Id,
            Description = task.Description,
            IsCritical = task.IsCritical,
            RequiredTools = task.RequiredTools,
            EstimatedDuration = task.EstimatedDuration
        }).ToList();

        return new ExecutionPlan
        {
            Id = Guid.NewGuid(),
            Steps = steps,
            TotalEstimatedDuration = CalculateTotalDuration(steps),
            CreatedAt = DateTime.UtcNow
        };
    }

    private async Task<SubtaskResult> ExecuteStepAsync(ExecutionStep step, AgentContext context)
    {
        _logger.LogInformation($"Executing step {step.Order}: {step.Description}");
        
        var startTime = DateTime.UtcNow;
        
        try
        {
            // Use appropriate tool based on step requirements
            var tool = context.Tools.GetTool(step.RequiredTools.FirstOrDefault() ?? "default");
            var result = await tool.ExecuteAsync(new Dictionary<string, object>
            {
                ["task"] = step.Description,
                ["context"] = context
            });

            return new SubtaskResult
            {
                SubtaskId = step.SubtaskId,
                Success = true,
                Output = result?.ToString(),
                ExecutionTime = DateTime.UtcNow - startTime,
                IsCritical = step.IsCritical
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Step {step.Order} failed");
            return new SubtaskResult
            {
                SubtaskId = step.SubtaskId,
                Success = false,
                ErrorMessage = ex.Message,
                ExecutionTime = DateTime.UtcNow - startTime,
                IsCritical = step.IsCritical
            };
        }
    }

    private async Task<string> SynthesizeResultsAsync(
        AgentTask task,
        List<SubtaskResult> results,
        AgentContext context)
    {
        var successfulResults = results.Where(r => r.Success).ToList();
        var failedResults = results.Where(r => !r.Success).ToList();

        var prompt = $$"""
        Original Task: {{task.Objective}}
        
        Completed Steps ({{successfulResults.Count}}):
        {{string.Join("\n", successfulResults.Select(r => $"- {r.Output}"))}}
        
        {{(failedResults.Any() ? $"Failed Steps ({failedResults.Count}):\n{string.Join("\n", failedResults.Select(r => $"- {r.ErrorMessage}"))}" : "")}}
        
        Provide a comprehensive summary of what was accomplished:
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.5,
            MaxTokens = 800
        });

        return response.Text;
    }

    private List<Subtask> TopologicalSort(List<Subtask> subtasks)
    {
        // Simple topological sort implementation
        var sorted = new List<Subtask>();
        var visited = new HashSet<int>();
        var visiting = new HashSet<int>();

        void Visit(Subtask task)
        {
            if (visited.Contains(task.Id)) return;
            if (visiting.Contains(task.Id))
                throw new InvalidOperationException("Circular dependency detected");

            visiting.Add(task.Id);

            foreach (var depId in task.Dependencies)
            {
                var dep = subtasks.FirstOrDefault(t => t.Id == depId);
                if (dep != null) Visit(dep);
            }

            visiting.Remove(task.Id);
            visited.Add(task.Id);
            sorted.Add(task);
        }

        foreach (var task in subtasks)
        {
            Visit(task);
        }

        return sorted;
    }

    private TimeSpan CalculateTotalDuration(List<ExecutionStep> steps)
    {
        return TimeSpan.FromSeconds(
            steps.Sum(s => ParseDuration(s.EstimatedDuration).TotalSeconds));
    }

    private TimeSpan ParseDuration(string duration)
    {
        // Simple parser: "5m", "30s", "1h"
        var value = int.Parse(new string(duration.Where(char.IsDigit).ToArray()));
        var unit = duration.Last();

        return unit switch
        {
            's' => TimeSpan.FromSeconds(value),
            'm' => TimeSpan.FromMinutes(value),
            'h' => TimeSpan.FromHours(value),
            _ => TimeSpan.FromMinutes(value)
        };
    }

    private double CalculatePlanConfidence(List<SubtaskResult> results)
    {
        if (!results.Any()) return 0.0;

        var totalWeight = results.Sum(r => r.IsCritical ? 2.0 : 1.0);
        var successWeight = results.Where(r => r.Success).Sum(r => r.IsCritical ? 2.0 : 1.0);

        return successWeight / totalWeight;
    }

    public override async Task<ReflectionResult> ReflectOnResultAsync(AgentResult result)
    {
        var improvements = new List<string>();
        
        if (result.Metadata.ContainsKey("failed_steps"))
        {
            var failedCount = (int)result.Metadata["failed_steps"];
            if (failedCount > 0)
            {
                improvements.Add($"Improve error handling for {failedCount} failed steps");
                improvements.Add("Consider adding fallback strategies for critical steps");
            }
        }

        return new ReflectionResult
        {
            ShouldRetry = !result.Success && result.ConfidenceScore > 0.3,
            Insights = "Plan execution completed with measurable results",
            Improvements = improvements,
            Learnings = new Dictionary<string, object>
            {
                ["avg_confidence"] = result.ConfidenceScore,
                ["execution_time"] = result.ExecutionTime.TotalSeconds
            }
        };
    }
}

// Supporting models
public record DecompositionResult(List<Subtask> Subtasks);

public class Subtask
{
    public int Id { get; set; }
    public string Description { get; set; }
    public List<int> Dependencies { get; set; } = new();
    public string EstimatedDuration { get; set; }
    public bool IsCritical { get; set; }
    public List<string> RequiredTools { get; set; } = new();
}

public class ExecutionPlan
{
    public Guid Id { get; set; }
    public List<ExecutionStep> Steps { get; set; }
    public TimeSpan TotalEstimatedDuration { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ExecutionStep
{
    public int Order { get; set; }
    public int SubtaskId { get; set; }
    public string Description { get; set; }
    public bool IsCritical { get; set; }
    public List<string> RequiredTools { get; set; }
    public string EstimatedDuration { get; set; }
}

public class SubtaskResult
{
    public int SubtaskId { get; set; }
    public bool Success { get; set; }
    public string Output { get; set; }
    public string ErrorMessage { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public bool IsCritical { get; set; }
}
