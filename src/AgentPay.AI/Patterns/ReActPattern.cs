using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Services;
using AgentPay.AI.Tools;

namespace AgentPay.AI.Patterns;

/// <summary>
/// Pattern 4: ReAct (Reasoning + Acting)
/// Interleaves reasoning traces with task-specific actions
/// </summary>
public class ReActPattern
{
    private readonly ILLMService _llm;
    private readonly IToolRegistry _tools;

    public ReActPattern(ILLMService llm, IToolRegistry tools)
    {
        _llm = llm;
        _tools = tools;
    }

    public async Task<ReActResult> ExecuteAsync(string task, int maxIterations = 5)
    {
        var steps = new List<ReActStep>();
        var conversationHistory = new List<string> { $"Task: {task}" };

        for (int i = 0; i < maxIterations; i++)
        {
            // Thought
            var thought = await GenerateThoughtAsync(task, conversationHistory);
            conversationHistory.Add($"Thought {i + 1}: {thought}");

            // Action
            var action = await DecideActionAsync(thought, conversationHistory);
            conversationHistory.Add($"Action {i + 1}: {action.ToolName}({string.Join(", ", action.Parameters)})");

            // Observation
            var observation = await ExecuteActionAsync(action);
            conversationHistory.Add($"Observation {i + 1}: {observation}");

            steps.Add(new ReActStep
            {
                Thought = thought,
                Action = action,
                Observation = observation
            });

            // Check if task is complete
            if (await IsTaskCompleteAsync(task, observation))
            {
                break;
            }
        }

        return new ReActResult
        {
            Task = task,
            Steps = steps,
            FinalAnswer = steps.Last().Observation.ToString(),
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<string> GenerateThoughtAsync(string task, List<string> history)
    {
        var prompt = $"""
        {string.Join("\n", history)}

        Think step-by-step about what to do next to accomplish the task.
        Thought:
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions { Temperature = 0.7 });
        return response.Text.Trim();
    }

    private async Task<ToolAction> DecideActionAsync(string thought, List<string> history)
    {
        var availableTools = _tools.GetAvailableTools();
        var toolsDesc = string.Join("\n", availableTools.Select(t => $"- {t.Name}: {t.Description}"));

        var prompt = $$"""
        {{string.Join("\n", history)}}

        Available tools:
        {{toolsDesc}}

        Based on your thought, what action should you take?
        Respond in JSON: {"toolName": "...", "parameters": {} }
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.3,
            ResponseFormat = "json"
        });

        return System.Text.Json.JsonSerializer.Deserialize<ToolAction>(response.Text);
    }

    private async Task<object> ExecuteActionAsync(ToolAction action)
    {
        return await _tools.ExecuteAsync(action.ToolName, action.Parameters);
    }

    private async Task<bool> IsTaskCompleteAsync(string task, object observation)
    {
        var prompt = $"""
        Original task: {task}
        Latest observation: {observation}

        Is the task complete? Answer 'yes' or 'no'.
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.1,
            MaxTokens = 10
        });

        return response.Text.ToLower().Contains("yes");
    }
}

public class ReActStep
{
    public string Thought { get; set; }
    public ToolAction Action { get; set; }
    public object Observation { get; set; }
}

public class ToolAction
{
    public string ToolName { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class ReActResult
{
    public string Task { get; set; }
    public List<ReActStep> Steps { get; set; }
    public string FinalAnswer { get; set; }
    public DateTime Timestamp { get; set; }
}
