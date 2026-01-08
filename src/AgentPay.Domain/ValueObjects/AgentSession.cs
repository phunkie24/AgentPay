using System;
using System.Collections.Generic;

namespace AgentPay.Domain.ValueObjects;

public class AgentSession
{
    public Guid Id { get; private set; }
    public Guid AgentId { get; private set; }
    public string Purpose { get; private set; }
    public SessionStatus Status { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public List<SessionMessage> Messages { get; private set; } = new();

    private AgentSession() { }

    public static AgentSession Create(Guid agentId, string purpose)
    {
        return new AgentSession
        {
            Id = Guid.NewGuid(),
            AgentId = agentId,
            Purpose = purpose,
            Status = SessionStatus.Active,
            StartedAt = DateTime.UtcNow
        };
    }

    public void AddMessage(string role, string content)
    {
        Messages.Add(new SessionMessage(role, content, DateTime.UtcNow));
    }

    public void End()
    {
        Status = SessionStatus.Completed;
        EndedAt = DateTime.UtcNow;
    }
}

public enum SessionStatus
{
    Active,
    Completed,
    Cancelled
}

public record SessionMessage(string Role, string Content, DateTime Timestamp);
