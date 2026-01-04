using System;
using System.Collections.Generic;

namespace AgentPay.Application.DTOs;

public class ReflectionDto
{
    public Guid Id { get; set; }
    public Guid AgentId { get; set; }
    public string ActionType { get; set; }
    public string ActionDescription { get; set; }
    public bool OutcomeSuccess { get; set; }
    public string OutcomeResult { get; set; }
    public string Insights { get; set; }
    public List<string> Learnings { get; set; } = new();
    public DateTime Timestamp { get; set; }
}
