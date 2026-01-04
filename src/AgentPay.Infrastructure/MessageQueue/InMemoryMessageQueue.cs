using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace AgentPay.Infrastructure.MessageQueue;

/// <summary>
/// In-memory message queue implementation for development and testing
/// NOT suitable for production or distributed systems
/// </summary>
public class InMemoryMessageQueue : IMessageQueue
{
    private readonly ConcurrentDictionary<string, List<Func<object, Task>>> _subscriptions;
    private readonly ConcurrentDictionary<string, ConcurrentQueue<object>> _messageStore;

    public InMemoryMessageQueue()
    {
        _subscriptions = new ConcurrentDictionary<string, List<Func<object, Task>>>();
        _messageStore = new ConcurrentDictionary<string, ConcurrentQueue<object>>();
    }

    public async Task PublishAsync<T>(string topic, T message) where T : class
    {
        // Store message
        var queue = _messageStore.GetOrAdd(topic, _ => new ConcurrentQueue<object>());
        queue.Enqueue(message);

        // Limit queue size to prevent memory leaks
        while (queue.Count > 1000)
        {
            queue.TryDequeue(out _);
        }

        // Notify subscribers
        if (_subscriptions.TryGetValue(topic, out var handlers))
        {
            var tasks = handlers.Select(handler => handler(message));
            await Task.WhenAll(tasks);
        }
    }

    public Task SubscribeAsync<T>(string topic, Func<T, Task> handler) where T : class
    {
        var wrappedHandler = new Func<object, Task>(async obj =>
        {
            if (obj is T typedMessage)
            {
                await handler(typedMessage);
            }
        });

        _subscriptions.AddOrUpdate(
            topic,
            _ => new List<Func<object, Task>> { wrappedHandler },
            (_, existing) =>
            {
                existing.Add(wrappedHandler);
                return existing;
            });

        return Task.CompletedTask;
    }

    public Task UnsubscribeAsync(string topic)
    {
        _subscriptions.TryRemove(topic, out _);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<T>> GetMessagesAsync<T>(string topic, int maxCount = 10) where T : class
    {
        if (!_messageStore.TryGetValue(topic, out var queue))
        {
            return Task.FromResult(Enumerable.Empty<T>());
        }

        var messages = queue
            .OfType<T>()
            .Take(maxCount)
            .ToList();

        return Task.FromResult<IEnumerable<T>>(messages);
    }
}
