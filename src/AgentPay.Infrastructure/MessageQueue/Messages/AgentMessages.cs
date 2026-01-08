using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgentPay.Infrastructure.MessageQueue.Messages;

/// <summary>
/// Agent lifecycle messages
/// </summary>
public class AgentCreatedMessage : QueueMessage
{
    public Guid AgentId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;

    public AgentCreatedMessage()
    {
        MessageType = nameof(AgentCreatedMessage);
    }
}

public class AgentActivatedMessage : QueueMessage
{
    public Guid AgentId { get; init; }

    public AgentActivatedMessage()
    {
        MessageType = nameof(AgentActivatedMessage);
    }
}

public class AgentDeactivatedMessage : QueueMessage
{
    public Guid AgentId { get; init; }

    public AgentDeactivatedMessage()
    {
        MessageType = nameof(AgentDeactivatedMessage);
    }
}

/// <summary>
/// Agent balance update message
/// </summary>
public class AgentBalanceUpdatedMessage : QueueMessage
{
    public Guid AgentId { get; init; }
    public decimal OldBalance { get; init; }
    public decimal NewBalance { get; init; }

    public AgentBalanceUpdatedMessage()
    {
        MessageType = nameof(AgentBalanceUpdatedMessage);
    }
}

/// <summary>
/// Agent session messages
/// </summary>
public class SessionStartedMessage : QueueMessage
{
    public Guid AgentId { get; init; }
    public Guid SessionId { get; init; }
    public string Purpose { get; init; } = string.Empty;

    public SessionStartedMessage()
    {
        MessageType = nameof(SessionStartedMessage);
    }
}

public class SessionEndedMessage : QueueMessage
{
    public Guid AgentId { get; init; }
    public Guid SessionId { get; init; }

    public SessionEndedMessage()
    {
        MessageType = nameof(SessionEndedMessage);
    }
}
