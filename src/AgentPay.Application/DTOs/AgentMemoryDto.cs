using System;
using System.Collections.Generic;

namespace AgentPay.Application.DTOs;

public class AgentMemoryDto
{
    public Guid AgentId { get; set; }
    public List<ThoughtDto> RecentThoughts { get; set; } = new();
    public List<ReflectionDto> Reflections { get; set; } = new();
    public int TotalMemoryEntries { get; set; }
    public DateTime LastUpdated { get; set; }
}
