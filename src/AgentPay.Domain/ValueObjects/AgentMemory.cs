using System;
using System.Collections.Generic;
using System.Linq;

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
