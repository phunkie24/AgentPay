using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgentPay.Infrastructure.MessageQueue.Messages;

/// <summary>
/// Transaction lifecycle messages
/// </summary>
public class TransactionInitiatedMessage : QueueMessage
{
    public Guid TransactionId { get; init; }
    public Guid AgentId { get; init; }
    public Guid ServiceId { get; init; }
    public decimal Amount { get; init; }

    public TransactionInitiatedMessage()
    {
        MessageType = nameof(TransactionInitiatedMessage);
    }
}

public class TransactionSubmittedMessage : QueueMessage
{
    public Guid TransactionId { get; init; }
    public string TransactionHash { get; init; } = string.Empty;

    public TransactionSubmittedMessage()
    {
        MessageType = nameof(TransactionSubmittedMessage);
    }
}

public class TransactionCompletedMessage : QueueMessage
{
    public Guid TransactionId { get; init; }
    public string TransactionHash { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public int GasUsed { get; init; }

    public TransactionCompletedMessage()
    {
        MessageType = nameof(TransactionCompletedMessage);
    }
}

public class TransactionFailedMessage : QueueMessage
{
    public Guid TransactionId { get; init; }
    public string Reason { get; init; } = string.Empty;

    public TransactionFailedMessage()
    {
        MessageType = nameof(TransactionFailedMessage);
    }
}

public class TransactionBlockedMessage : QueueMessage
{
    public Guid TransactionId { get; init; }
    public string Reason { get; init; } = string.Empty;

    public TransactionBlockedMessage()
    {
        MessageType = nameof(TransactionBlockedMessage);
    }
}
