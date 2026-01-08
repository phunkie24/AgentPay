using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgentPay.Application.DTOs;

namespace AgentPay.Application.Services;

/// <summary>
/// Manages agent memory, thoughts, and reflections
/// </summary>
public interface IAgentMemoryService
{
    Task RecordThoughtAsync(Guid agentId, string thought, string reasoning);
    Task<List<ThoughtDto>> GetRecentThoughtsAsync(Guid agentId, int count = 10);
    Task RecordReflectionAsync(Guid agentId, ReflectionDto reflection);
    Task<List<ReflectionDto>> GetReflectionsAsync(Guid agentId);
    Task<AgentMemoryDto> GetMemorySummaryAsync(Guid agentId);
}
