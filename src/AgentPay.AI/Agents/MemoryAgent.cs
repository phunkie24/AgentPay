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

namespace AgentPay.AI.Agents;

/// <summary>
/// Memory Agent: Context retention and recall
/// Implements: Pattern 26 (Session Memory), Pattern 28 (Long-Term Memory)
/// </summary>
public class MemoryAgent : BaseAgent
{
    private readonly ILLMService _llm;
    private readonly IVectorStoreService _vectorStore;
    private readonly ILogger<MemoryAgent> _logger;

    public MemoryAgent(
        ILLMService llm,
        IVectorStoreService vectorStore,
        IToolRegistry toolRegistry,
        ILogger<MemoryAgent> logger)
        : base(toolRegistry, logger)
    {
        _llm = llm;
        _vectorStore = vectorStore;
        _logger = logger;
        Name = "Memory Agent";
        Role = AgentRole.Coordinator;
    }

    public override async Task<AgentResult> ExecuteAsync(AgentTask task, AgentContext context)
    {
        var operation = task.Parameters["operation"].ToString();

        return operation switch
        {
            "store" => await StoreMemoryAsync(task, context),
            "recall" => await RecallMemoryAsync(task, context),
            "summarize" => await SummarizeSessionAsync(task, context),
            _ => throw new ArgumentException($"Unknown operation: {operation}")
        };
    }

    private async Task<AgentResult> StoreMemoryAsync(AgentTask task, AgentContext context)
    {
        var key = task.Parameters["key"].ToString();
        var value = task.Parameters["value"];
        var contextInfo = task.Parameters.GetValueOrDefault("context")?.ToString();

        // Store in long-term memory with embedding
        var embedding = await _vectorStore.CreateEmbeddingAsync($"{key}: {contextInfo}");
        context.LongTermMemory.Store(key, value, embedding);

        return new AgentResult
        {
            Success = true,
            Output = $"Memory stored: {key}",
            Reasoning = "Successfully stored in long-term memory"
        };
    }

    private async Task<AgentResult> RecallMemoryAsync(AgentTask task, AgentContext context)
    {
        var query = task.Parameters["query"].ToString();
        var topK = task.Parameters.GetValueOrDefault("top_k") as int? ?? 5;

        // Create query embedding
        var queryEmbedding = await _vectorStore.CreateEmbeddingAsync(query);

        // Search similar memories
        var memories = context.LongTermMemory.SearchByEmbedding(queryEmbedding, topK);

        return new AgentResult
        {
            Success = true,
            Output = System.Text.Json.JsonSerializer.Serialize(memories),
            Reasoning = $"Found {memories.Count} relevant memories",
            Metadata = new Dictionary<string, object> { ["count"] = memories.Count }
        };
    }

    private async Task<AgentResult> SummarizeSessionAsync(AgentTask task, AgentContext context)
    {
        var messages = context.SessionMemory.GetMessages();

        var prompt = $"""
        Summarize this conversation session:
        
        {string.Join("\n", messages.Select(m => $"{m.Role}: {m.Content}"))}
        
        Provide a concise summary highlighting key points and decisions.
        """;

        var response = await _llm.GenerateAsync(prompt);

        return new AgentResult
        {
            Success = true,
            Output = response.Text,
            Reasoning = "Session summarized"
        };
    }

    public override Task<ReflectionResult> ReflectOnResultAsync(AgentResult result)
    {
        return Task.FromResult(new ReflectionResult
        {
            ShouldRetry = false,
            Insights = "Memory operations completed",
            Improvements = new List<string>()
        });
    }
}

public interface IVectorStoreService
{
    Task<string> CreateEmbeddingAsync(string text);
}
