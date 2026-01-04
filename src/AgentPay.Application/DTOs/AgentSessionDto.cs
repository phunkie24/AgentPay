using System;
using System.Collections.Generic;

namespace AgentPay.Application.DTOs;

public class AgentSessionDto
{
    public Guid Id { get; set; }
    public Guid AgentId { get; set; }
    public string Purpose { get; set; }
    public string Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public List<SessionMessageDto> Messages { get; set; } = new();
}

public class SessionMessageDto
{
    public string Role { get; set; }
    public string Content { get; set; }
    public DateTime Timestamp { get; set; }
}
