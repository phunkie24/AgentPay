using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Tools;
using AgentPay.Domain.Entities;
using AgentPay.Domain.ValueObjects;

namespace AgentPay.AI.Agents.Base;

/// <summary>
/// Base interface for all AI agents
/// Supports: Chain of Thought, Tool Calling, Reflection
/// </summary>
public interface IAgent
{
    string Name { get; }
    AgentRole Role { get; }
    
    // Pattern: Chain of Thought (Pattern 13)
    Task<AgentResult> ExecuteAsync(AgentTask task, AgentContext context);
    
    // Pattern: Reflection (Pattern 18)
    Task<ReflectionResult> ReflectOnResultAsync(AgentResult result);
    
    // Pattern: Self-Check (Pattern 31)
    Task<HealthCheck> PerformHealthCheckAsync();
}

/// <summary>
/// Agent context carrying session state and memory
/// Implements: Session Memory (Pattern 26), Context Window Management (Pattern 7)
/// </summary>
public class AgentContext
{
    public Guid SessionId { get; set; }
    public Guid AgentId { get; set; }
    
    // Pattern: Session Memory (Pattern 26)
    public ConversationMemory SessionMemory { get; set; }
    
    // Pattern: Long-Term Memory (Pattern 28)
    public LongTermMemory LongTermMemory { get; set; }
    
    // Pattern: Context Window Management (Pattern 7)
    public int MaxTokens { get; set; } = 8000;
    public int CurrentTokenCount { get; set; }
    
    // Available tools for this agent
    public IToolRegistry Tools { get; set; }
    
    // Guardrails policy
    public GuardrailsPolicy GuardrailsPolicy { get; set; }
    
    public Dictionary<string, object> Metadata { get; set; } = new();

    public AgentContext(Guid agentId, Guid sessionId)
    {
        AgentId = agentId;
        SessionId = sessionId;
        SessionMemory = new ConversationMemory();
        LongTermMemory = new LongTermMemory();
    }

    // Pattern: Context Window Management - Add message with token management
    public void AddMessage(string role, string content)
    {
        var tokenCount = EstimateTokens(content);
        
        // If adding this message would exceed the limit, summarize older messages
        if (CurrentTokenCount + tokenCount > MaxTokens)
        {
            SessionMemory.SummarizeOldMessages(MaxTokens / 2);
            CurrentTokenCount = SessionMemory.EstimateTokens();
        }

        SessionMemory.AddMessage(role, content);
        CurrentTokenCount += tokenCount;
    }

    private int EstimateTokens(string text)
    {
        // Rough estimation: ~4 characters per token
        return (int)Math.Ceiling(text.Length / 4.0);
    }
}

public class AgentTask
{
    public string Objective { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
    public TimeSpan? Timeout { get; set; }
    public int MaxRetries { get; set; } = 3;
}

public class AgentResult
{
    public bool Success { get; set; }
    public string Output { get; set; }
    public string Reasoning { get; set; } // Chain of Thought
    public List<ToolInvocation> ToolsUsed { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public TimeSpan ExecutionTime { get; set; }
    public string? ErrorMessage { get; set; }
    
    // Pattern: Self-Check (Pattern 31) - Confidence scores
    public double ConfidenceScore { get; set; }
    public Dictionary<string, double> TokenProbabilities { get; set; } = new();
}

public class ReflectionResult
{
    public bool ShouldRetry { get; set; }
    public string Insights { get; set; }
    public List<string> Improvements { get; set; } = new();
    public Dictionary<string, object> Learnings { get; set; } = new();
}

public class HealthCheck
{
    public bool IsHealthy { get; set; }
    public List<string> Issues { get; set; } = new();
    public DateTime CheckedAt { get; set; }
}

public class ToolInvocation
{
    public string ToolName { get; set; }
    public Dictionary<string, object> Parameters { get; set; }
    public object Result { get; set; }
    public TimeSpan ExecutionTime { get; set; }
}

/// <summary>
/// Conversation memory for session storage
/// Implements: Session Memory (Pattern 26)
/// </summary>
public class ConversationMemory
{
    private readonly List<Message> _messages = new();
    private readonly List<Message> _summarizedMessages = new();

    public void AddMessage(string role, string content)
    {
        _messages.Add(new Message(role, content, DateTime.UtcNow));
    }

    public List<Message> GetMessages() => _messages.ToList();

    public void SummarizeOldMessages(int targetTokenCount)
    {
        // Move older messages to summarized collection
        var toSummarize = _messages.Take(_messages.Count / 2).ToList();
        _summarizedMessages.AddRange(toSummarize);
        _messages.RemoveRange(0, toSummarize.Count);
    }

    public int EstimateTokens()
    {
        return _messages.Sum(m => (int)Math.Ceiling(m.Content.Length / 4.0));
    }
}

public record Message(string Role, string Content, DateTime Timestamp);

/// <summary>
/// Long-term memory with embeddings
/// Implements: Long-Term Memory (Pattern 28)
/// </summary>
public class LongTermMemory
{
    private readonly List<MemoryEntry> _entries = new();

    public void Store(string key, object value, string? embedding = null)
    {
        _entries.Add(new MemoryEntry(key, value, embedding, DateTime.UtcNow));
    }

    public object? Retrieve(string key)
    {
        return _entries.FirstOrDefault(e => e.Key == key)?.Value;
    }

    public List<MemoryEntry> SearchByEmbedding(string queryEmbedding, int topK = 5)
    {
        // In real implementation, this would use vector similarity
        return _entries.Where(e => e.Embedding != null).Take(topK).ToList();
    }
}

public record MemoryEntry(string Key, object Value, string? Embedding, DateTime StoredAt);
