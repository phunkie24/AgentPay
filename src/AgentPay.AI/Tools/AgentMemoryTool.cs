using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.Domain.Repositories;
using AgentPay.Domain.ValueObjects;

namespace AgentPay.AI.Tools;

/// <summary>
/// Tool for managing agent memory and context
/// </summary>
public class AgentMemoryTool : ITool
{
    private readonly IAgentRepository _agentRepo;

    public string Name => "agent_memory";
    public string Description => "Store and retrieve information from agent memory";

    public AgentMemoryTool(IAgentRepository agentRepo)
    {
        _agentRepo = agentRepo;
    }

    public async Task<object> ExecuteAsync(Dictionary<string, object> parameters)
    {
        var action = parameters.GetValueOrDefault("action")?.ToString();

        return action switch
        {
            "store" => await StoreMemoryAsync(parameters),
            "retrieve" => await RetrieveMemoryAsync(parameters),
            "add_thought" => await AddThoughtAsync(parameters),
            "get_thoughts" => await GetThoughtsAsync(parameters),
            _ => throw new ArgumentException($"Unknown action: {action}")
        };
    }

    private async Task<object> StoreMemoryAsync(Dictionary<string, object> parameters)
    {
        var agentId = Guid.Parse(parameters.GetValueOrDefault("agent_id")?.ToString() ?? throw new ArgumentException("agent_id required"));
        var key = parameters.GetValueOrDefault("key")?.ToString() ?? throw new ArgumentException("key required");
        var value = parameters.GetValueOrDefault("value") ?? throw new ArgumentException("value required");
        var context = parameters.GetValueOrDefault("context")?.ToString() ?? "";

        var agent = await _agentRepo.GetByIdAsync(agentId);
        if (agent == null)
            return new { error = "Agent not found" };

        agent.LongTermMemory.AddMemory(key, value, context);
        await _agentRepo.UpdateAsync(agent);

        return new
        {
            success = true,
            agentId,
            key,
            stored = DateTime.UtcNow
        };
    }

    private async Task<object> RetrieveMemoryAsync(Dictionary<string, object> parameters)
    {
        var agentId = Guid.Parse(parameters.GetValueOrDefault("agent_id")?.ToString() ?? throw new ArgumentException("agent_id required"));

        var agent = await _agentRepo.GetByIdAsync(agentId);
        if (agent == null)
            return new { error = "Agent not found" };

        var thoughts = agent.LongTermMemory.GetRecentThoughts();
        var reflections = agent.LongTermMemory.GetReflections();

        return new
        {
            success = true,
            agentId,
            thoughtCount = thoughts.Count,
            reflectionCount = reflections.Count,
            recentThoughts = thoughts.Select(t => new
            {
                thought = t.Thought,
                reasoning = t.Reasoning,
                timestamp = t.Timestamp
            }).ToList()
        };
    }

    private async Task<object> AddThoughtAsync(Dictionary<string, object> parameters)
    {
        var agentId = Guid.Parse(parameters.GetValueOrDefault("agent_id")?.ToString() ?? throw new ArgumentException("agent_id required"));
        var thought = parameters.GetValueOrDefault("thought")?.ToString() ?? throw new ArgumentException("thought required");
        var reasoning = parameters.GetValueOrDefault("reasoning")?.ToString() ?? "";

        var agent = await _agentRepo.GetByIdAsync(agentId);
        if (agent == null)
            return new { error = "Agent not found" };

        agent.LongTermMemory.AddThought(thought, reasoning, DateTime.UtcNow);
        await _agentRepo.UpdateAsync(agent);

        return new
        {
            success = true,
            agentId,
            thought,
            recorded = DateTime.UtcNow
        };
    }

    private async Task<object> GetThoughtsAsync(Dictionary<string, object> parameters)
    {
        var agentId = Guid.Parse(parameters.GetValueOrDefault("agent_id")?.ToString() ?? throw new ArgumentException("agent_id required"));
        var count = Convert.ToInt32(parameters.GetValueOrDefault("count") ?? 10);

        var agent = await _agentRepo.GetByIdAsync(agentId);
        if (agent == null)
            return new { error = "Agent not found" };

        var thoughts = agent.LongTermMemory.GetRecentThoughts(count);

        return new
        {
            success = true,
            agentId,
            thoughts = thoughts.Select(t => new
            {
                thought = t.Thought,
                reasoning = t.Reasoning,
                timestamp = t.Timestamp
            }).ToList()
        };
    }
}
