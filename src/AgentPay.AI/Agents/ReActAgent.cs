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
/// ReAct Agent: Combines Reasoning and Acting
/// Implements: Pattern 13 (Chain of Thought), Pattern 20 (Tool-Augmented Reasoning)
/// Flow: Thought -> Action -> Observation -> Reflection
/// </summary>
public class ReActAgent : BaseAgent
{
    private readonly ILLMService _llm;
    private readonly ILogger<ReActAgent> _logger;

    public ReActAgent(
        ILLMService llm,
        IToolRegistry toolRegistry,
        ILogger<ReActAgent> logger) 
        : base(toolRegistry, logger)
    {
        _llm = llm;
        _logger = logger;
        Name = "ReAct Agent";
        Role = AgentRole.Executor;
    }

    public override async Task<AgentResult> ExecuteAsync(AgentTask task, AgentContext context)
    {
        var startTime = DateTime.UtcNow;
        var thoughts = new List<string>();
        var actions = new List<ToolInvocation>();

        try
        {
            // Pattern: Chain of Thought (Pattern 13)
            // Step 1: THOUGHT - Reason about the task
            var thought = await GenerateThoughtAsync(task, context);
            thoughts.Add(thought);
            context.AddMessage("assistant", $"Thought: {thought}");

            // Step 2: ACTION - Decide what to do
            var actionPlan = await PlanActionAsync(thought, task, context);
            context.AddMessage("assistant", $"Action Plan: {JsonSerializer.Serialize(actionPlan)}");

            // Step 3: EXECUTE - Perform the action
            var observation = await ExecuteActionAsync(actionPlan, context);
            actions.Add(observation);
            context.AddMessage("assistant", $"Observation: {observation.Result}");

            // Step 4: REFLECTION - Did we succeed?
            var shouldContinue = await ShouldContinueAsync(task, observation, context);
            
            var maxIterations = 5;
            var iteration = 1;

            while (shouldContinue && iteration < maxIterations)
            {
                // Iterative ReAct loop
                thought = await GenerateThoughtAsync(task, context);
                thoughts.Add(thought);
                
                actionPlan = await PlanActionAsync(thought, task, context);
                observation = await ExecuteActionAsync(actionPlan, context);
                actions.Add(observation);
                
                shouldContinue = await ShouldContinueAsync(task, observation, context);
                iteration++;
            }

            // Final answer
            var finalAnswer = await GenerateFinalAnswerAsync(task, thoughts, actions, context);

            return new AgentResult
            {
                Success = true,
                Output = finalAnswer,
                Reasoning = string.Join("\n", thoughts),
                ToolsUsed = actions,
                ExecutionTime = DateTime.UtcNow - startTime,
                ConfidenceScore = CalculateConfidence(actions)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ReAct agent execution failed");
            return new AgentResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                Reasoning = string.Join("\n", thoughts),
                ToolsUsed = actions,
                ExecutionTime = DateTime.UtcNow - startTime
            };
        }
    }

    // Pattern: Chain of Thought - Generate reasoning
    private async Task<string> GenerateThoughtAsync(AgentTask task, AgentContext context)
    {
        var prompt = $$"""
        You are a reasoning AI agent working on the following task:
        {{task.Objective}}

        Previous context:
        {{string.Join("\n", context.SessionMemory.GetMessages().TakeLast(5).Select(m => $"{m.Role}: {m.Content}"))}}

        Think step by step about what you need to do next. What information do you need? What tools should you use?
        
        Provide your reasoning:
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.7,
            MaxTokens = 500
        });

        return response.Text;
    }

    // Pattern: Decomposition (Pattern 12) - Break down into actions
    private async Task<ActionPlan> PlanActionAsync(string thought, AgentTask task, AgentContext context)
    {
        var availableTools = context.Tools.GetAvailableTools();
        var toolsDescription = string.Join("\n", availableTools.Select(t => 
            $"- {t.Name}: {t.Description}"));

        var prompt = $$"""
        Based on your reasoning:
        {{thought}}

        Available tools:
        {{toolsDescription}}

        Decide which tool to use and with what parameters. Respond in JSON format:
        {
            "toolName": "tool_name",
            "parameters": { "key": "value" },
            "reasoning": "why you chose this tool"
        }
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.3,
            ResponseFormat = "json"
        });

        return JsonSerializer.Deserialize<ActionPlan>(response.Text);
    }

    // Pattern: Tool Calling (Pattern 21)
    private async Task<ToolInvocation> ExecuteActionAsync(ActionPlan plan, AgentContext context)
    {
        var tool = context.Tools.GetTool(plan.ToolName);
        var startTime = DateTime.UtcNow;
        
        var result = await tool.ExecuteAsync(plan.Parameters);
        
        return new ToolInvocation
        {
            ToolName = plan.ToolName,
            Parameters = plan.Parameters,
            Result = result,
            ExecutionTime = DateTime.UtcNow - startTime
        };
    }

    private async Task<bool> ShouldContinueAsync(
        AgentTask task, 
        ToolInvocation observation, 
        AgentContext context)
    {
        var prompt = $$"""
        Task: {{task.Objective}}
        Latest observation: {{observation.Result}}

        Has the task been completed successfully? Answer with just 'yes' or 'no'.
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.1,
            MaxTokens = 10
        });

        return !response.Text.ToLower().Contains("yes");
    }

    private async Task<string> GenerateFinalAnswerAsync(
        AgentTask task,
        List<string> thoughts,
        List<ToolInvocation> actions,
        AgentContext context)
    {
        var prompt = $$"""
        Task: {{task.Objective}}

        Your reasoning process:
        {{string.Join("\n", thoughts)}}

        Actions taken:
        {{string.Join("\n", actions.Select(a => $"- {a.ToolName}: {a.Result}"))}}

        Provide a clear, concise final answer to the task:
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.5,
            MaxTokens = 1000
        });

        return response.Text;
    }

    // Pattern: Self-Check (Pattern 31) - Calculate confidence
    private double CalculateConfidence(List<ToolInvocation> actions)
    {
        if (!actions.Any()) return 0.0;
        
        // Simple heuristic: more successful actions = higher confidence
        var successfulActions = actions.Count(a => a.Result != null);
        return (double)successfulActions / actions.Count;
    }

    public override async Task<ReflectionResult> ReflectOnResultAsync(AgentResult result)
    {
        var prompt = $$"""
        You executed a task with the following result:
        Success: {{result.Success}}
        Output: {{result.Output}}
        Reasoning: {{result.Reasoning}}
        
        Reflect on this execution:
        1. What went well?
        2. What could be improved?
        3. What did you learn?
        
        Respond in JSON format:
        {
            "shouldRetry": false,
            "insights": "...",
            "improvements": ["..."],
            "learnings": {}
        }
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.6,
            ResponseFormat = "json"
        });

        return JsonSerializer.Deserialize<ReflectionResult>(response.Text);
    }
}

public record ActionPlan(
    string ToolName,
    Dictionary<string, object> Parameters,
    string Reasoning);
