using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgentPay.Infrastructure.MessageQueue.Messages;

/// <summary>
/// Payment session messages
/// </summary>
public class PaymentSessionStartedMessage : QueueMessage
{
    public Guid SessionId { get; init; }
    public Guid AgentId { get; init; }
    public Guid ServiceId { get; init; }
    public decimal BudgetLimit { get; init; }

    public PaymentSessionStartedMessage()
    {
        MessageType = nameof(PaymentSessionStartedMessage);
    }
}

public class PaymentNegotiationCompletedMessage : QueueMessage
{
    public Guid SessionId { get; init; }
    public decimal OriginalPrice { get; init; }
    public decimal NegotiatedPrice { get; init; }

    public PaymentNegotiationCompletedMessage()
    {
        MessageType = nameof(PaymentNegotiationCompletedMessage);
    }
}

public class PaymentSessionCompletedMessage : QueueMessage
{
    public Guid SessionId { get; init; }
    public Guid TransactionId { get; init; }
    public decimal FinalAmount { get; init; }

    public PaymentSessionCompletedMessage()
    {
        MessageType = nameof(PaymentSessionCompletedMessage);
    }
}

public class PaymentSessionFailedMessage : QueueMessage
{
    public Guid SessionId { get; init; }
    public string Reason { get; init; } = string.Empty;

    public PaymentSessionFailedMessage()
    {
        MessageType = nameof(PaymentSessionFailedMessage);
    }
}
