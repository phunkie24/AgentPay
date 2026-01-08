using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Tools;
using AgentPay.Domain.Entities;
using AgentPay.Domain.ValueObjects;
using AgentPay.AI.Agents.Base;
using AgentPay.AI.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AgentPay.AI.Agents;

/// <summary>
/// Reflection Agent: Self-improvement and learning
/// Implements: Pattern 18 (Reflection), Pattern 28 (Long-Term Memory)
/// </summary>
public class ReflectionAgent : BaseAgent
{
    private readonly ILLMService _llm;
    private readonly ILogger<ReflectionAgent> _logger;

    public ReflectionAgent(
        ILLMService llm,
        IToolRegistry toolRegistry,
        ILogger<ReflectionAgent> logger)
        : base(toolRegistry, logger)
    {
        _llm = llm;
        _logger = logger;
        Name = "Reflection Agent";
        Role = AgentRole.Reflector;
    }

    public override async Task<AgentResult> ExecuteAsync(AgentTask task, AgentContext context)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            var actionResult = task.Parameters["action_result"] as AgentResult;
            var actionType = task.Parameters["action_type"].ToString();

            // Pattern: Reflection (Pattern 18) - Deep analysis
            var reflection = await PerformReflectionAsync(actionResult, actionType, context);

            // Extract learnings
            var learnings = await ExtractLearningsAsync(reflection, context);

            // Update strategy recommendations
            var recommendations = await GenerateRecommendationsAsync(
                learnings, actionResult, context);

            // Store in long-term memory
            await StoreLearningsAsync(learnings, context);

            return new AgentResult
            {
                Success = true,
                Output = JsonSerializer.Serialize(new
                {
                    Reflection = reflection,
                    Learnings = learnings,
                    Recommendations = recommendations
                }),
                Reasoning = $"Reflected on {actionType} action. " +
                           $"Extracted {learnings.Count} insights.",
                ExecutionTime = DateTime.UtcNow - startTime,
                Metadata = new Dictionary<string, object>
                {
                    ["reflection_depth"] = "deep",
                    ["insights_count"] = learnings.Count
                },
                ConfidenceScore = 0.95
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Reflection failed");
            return new AgentResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ExecutionTime = DateTime.UtcNow - startTime
            };
        }
    }

    private async Task<string> PerformReflectionAsync(
        AgentResult actionResult,
        string actionType,
        AgentContext context)
    {
        var prompt = $$"""
        Reflect deeply on this action and its outcome:

        Action Type: {{actionType}}
        Success: {{actionResult.Success}}
        Output: {{actionResult.Output}}
        Reasoning: {{actionResult.Reasoning}}
        Execution Time: {{actionResult.ExecutionTime.TotalSeconds}}s
        Confidence: {{actionResult.ConfidenceScore}}

        Analyze:
        1. What went well?
        2. What could be improved?
        3. What patterns do you notice?
        4. What would you do differently next time?
        5. What general principles apply?

        Provide thoughtful reflection (3-5 sentences):
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.7,
            MaxTokens = 500
        });

        return response.Text;
    }

    private async Task<List<Learning>> ExtractLearningsAsync(
        string reflection,
        AgentContext context)
    {
        var prompt = $$"""
        From this reflection:
        {{reflection}}

        Extract concrete, actionable learnings in JSON format:
        [
            {
                "principle": "...",
                "application": "...",
                "confidence": 0.8
            }
        ]

        Focus on generalizable principles.
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.5,
            ResponseFormat = "json"
        });

        try
        {
            return JsonSerializer.Deserialize<List<Learning>>(response.Text) 
                ?? new List<Learning>();
        }
        catch
        {
            return new List<Learning>();
        }
    }

    private async Task<List<string>> GenerateRecommendationsAsync(
        List<Learning> learnings,
        AgentResult actionResult,
        AgentContext context)
    {
        var prompt = $$"""
        Based on these learnings:
        {{JsonSerializer.Serialize(learnings)}}

        And this action result (Success: {{actionResult.Success}}):
        {{actionResult.Output}}

        Generate 3-5 specific recommendations for future actions.
        Be concrete and actionable.

        Format as JSON array of strings:
        ["recommendation 1", "recommendation 2", ...]
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.6,
            ResponseFormat = "json"
        });

        try
        {
            return JsonSerializer.Deserialize<List<string>>(response.Text) 
                ?? new List<string>();
        }
        catch
        {
            return new List<string> { "Continue monitoring performance" };
        }
    }

    private async Task StoreLearningsAsync(
        List<Learning> learnings,
        AgentContext context)
    {
        // Pattern: Long-Term Memory (Pattern 28)
        foreach (var learning in learnings)
        {
            context.LongTermMemory.Store(
                $"learning_{Guid.NewGuid()}",
                learning,
                null
            );
        }

        await Task.CompletedTask;
    }

    public override async Task<ReflectionResult> ReflectOnResultAsync(AgentResult result)
    {
        // Meta-reflection: reflecting on reflection
        return new ReflectionResult
        {
            ShouldRetry = false,
            Insights = "Reflection completed successfully",
            Improvements = new() { "Continue deep reflection practice" },
            Learnings = new Dictionary<string, object>
            {
                ["reflection_effective"] = true
            }
        };
    }
}

public class Learning
{
    public string Principle { get; set; }
    public string Application { get; set; }
    public double Confidence { get; set; }
}
