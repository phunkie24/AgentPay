using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using StackExchange.Redis;
using System.Text.Json;

namespace AgentPay.Infrastructure.MessageQueue;

/// <summary>
/// Redis-based message queue implementation using Pub/Sub
/// Provides real-time event distribution across agents
/// </summary>
public class RedisMessageQueue : IMessageQueue
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ISubscriber _subscriber;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly Dictionary<string, List<object>> _handlers;

    public RedisMessageQueue(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _subscriber = redis.GetSubscriber();
        _handlers = new Dictionary<string, List<object>>();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task PublishAsync<T>(string topic, T message) where T : class
    {
        var json = JsonSerializer.Serialize(message, _jsonOptions);
        await _subscriber.PublishAsync(topic, json);
    }

    public async Task SubscribeAsync<T>(string topic, Func<T, Task> handler) where T : class
    {
        if (!_handlers.ContainsKey(topic))
        {
            _handlers[topic] = new List<object>();

            await _subscriber.SubscribeAsync(topic, async (channel, message) =>
            {
                var deserializedMessage = JsonSerializer.Deserialize<T>(message!, _jsonOptions);

                if (deserializedMessage != null && _handlers.ContainsKey(topic))
                {
                    foreach (var h in _handlers[topic].Cast<Func<T, Task>>())
                    {
                        await h(deserializedMessage);
                    }
                }
            });
        }

        _handlers[topic].Add(handler);
    }

    public async Task UnsubscribeAsync(string topic)
    {
        if (_handlers.ContainsKey(topic))
        {
            await _subscriber.UnsubscribeAsync(topic);
            _handlers.Remove(topic);
        }
    }

    public Task<IEnumerable<T>> GetMessagesAsync<T>(string topic, int maxCount = 10) where T : class
    {
        // Redis Pub/Sub doesn't support message retrieval
        // For persistent queues, use Redis Streams instead
        throw new NotSupportedException("Redis Pub/Sub does not support message retrieval. Use Redis Streams for persistent queuing.");
    }
}
