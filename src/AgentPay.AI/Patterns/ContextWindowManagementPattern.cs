using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Services;

namespace AgentPay.AI.Patterns;

/// <summary>
/// Pattern 27: Context Window Management
/// Manages long contexts through summarization and prioritization
/// </summary>
public class ContextWindowManagementPattern
{
    private readonly ILLMService _llm;
    private readonly int _maxTokens;

    public ContextWindowManagementPattern(ILLMService llm, int maxTokens = 8000)
    {
        _llm = llm;
        _maxTokens = maxTokens;
    }

    public async Task<ContextManagementResult> ExecuteAsync(
        string currentTask,
        List<ContextChunk> contextHistory)
    {
        var totalTokens = EstimateTokens(contextHistory);

        List<ContextChunk> managedContext;
        string strategy;

        if (totalTokens <= _maxTokens)
        {
            managedContext = contextHistory;
            strategy = "full_context";
        }
        else
        {
            managedContext = await ManageContextAsync(contextHistory, currentTask);
            strategy = "compressed_context";
        }

        var response = await ExecuteWithContextAsync(currentTask, managedContext);

        return new ContextManagementResult
        {
            CurrentTask = currentTask,
            OriginalTokenCount = totalTokens,
            ManagedTokenCount = EstimateTokens(managedContext),
            Strategy = strategy,
            ManagedContext = managedContext,
            Response = response,
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<List<ContextChunk>> ManageContextAsync(
        List<ContextChunk> context,
        string currentTask)
    {
        // Prioritize recent and relevant context
        var recentContext = context.TakeLast(5).ToList();
        var olderContext = context.SkipLast(5).ToList();

        // Summarize older context
        if (olderContext.Any())
        {
            var summary = await SummarizeContextAsync(olderContext, currentTask);
            var summarizedChunk = new ContextChunk
            {
                Content = summary,
                Type = "summary",
                Timestamp = DateTime.UtcNow
            };

            return new List<ContextChunk> { summarizedChunk }.Concat(recentContext).ToList();
        }

        return recentContext;
    }

    private async Task<string> SummarizeContextAsync(
        List<ContextChunk> chunks,
        string currentTask)
    {
        var contextText = string.Join("\n\n", chunks.Select(c => c.Content));

        var prompt = $"""
        Current task: {currentTask}

        Summarize this context, focusing on information relevant to the current task:
        {contextText}

        Summary:
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.3,
            MaxTokens = 500
        });

        return response.Text.Trim();
    }

    private async Task<string> ExecuteWithContextAsync(
        string task,
        List<ContextChunk> context)
    {
        var contextText = string.Join("\n\n", context.Select(c => $"[{c.Type}] {c.Content}"));

        var prompt = $"""
        Context:
        {contextText}

        Current Task: {task}

        Response:
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.6
        });

        return response.Text.Trim();
    }

    private int EstimateTokens(List<ContextChunk> chunks)
    {
        return chunks.Sum(c => EstimateTokens(c.Content));
    }

    private int EstimateTokens(string text)
    {
        return (int)Math.Ceiling(text.Length / 4.0);
    }
}

public class ContextChunk
{
    public string Content { get; set; }
    public string Type { get; set; } // message, summary, data, etc.
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class ContextManagementResult
{
    public string CurrentTask { get; set; }
    public int OriginalTokenCount { get; set; }
    public int ManagedTokenCount { get; set; }
    public string Strategy { get; set; }
    public List<ContextChunk> ManagedContext { get; set; }
    public string Response { get; set; }
    public DateTime Timestamp { get; set; }
}
