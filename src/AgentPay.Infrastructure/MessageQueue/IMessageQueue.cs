using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgentPay.Infrastructure.MessageQueue;

/// <summary>
/// Message queue interface for async communication between agents and services
/// Supports pub/sub pattern for event-driven architecture
/// </summary>
public interface IMessageQueue
{
    Task PublishAsync<T>(string topic, T message) where T : class;
    Task SubscribeAsync<T>(string topic, Func<T, Task> handler) where T : class;
    Task UnsubscribeAsync(string topic);
    Task<IEnumerable<T>> GetMessagesAsync<T>(string topic, int maxCount = 10) where T : class;
}

/// <summary>
/// Base message class for all queue messages
/// </summary>
public abstract class QueueMessage
{
    public Guid MessageId { get; init; } = Guid.NewGuid();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string MessageType { get; init; } = string.Empty;
}
