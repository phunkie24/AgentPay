using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;

namespace AgentPay.Infrastructure.Caching;

/// <summary>
/// In-memory cache implementation using IMemoryCache
/// Suitable for single-instance deployments and development
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly HashSet<string> _keys;
    private readonly object _lockObject = new();

    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
        _keys = new HashSet<string>();
    }

    public Task<T?> GetAsync<T>(string key) where T : class
    {
        _cache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        var options = new MemoryCacheEntryOptions();

        if (expiration.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = expiration.Value;
        }

        options.RegisterPostEvictionCallback((k, v, r, s) =>
        {
            lock (_lockObject)
            {
                _keys.Remove(k.ToString()!);
            }
        });

        _cache.Set(key, value, options);

        lock (_lockObject)
        {
            _keys.Add(key);
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key)
    {
        return Task.FromResult(_cache.TryGetValue(key, out _));
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);

        lock (_lockObject)
        {
            _keys.Remove(key);
        }

        return Task.CompletedTask;
    }

    public Task RemoveByPatternAsync(string pattern)
    {
        List<string> keysToRemove;

        lock (_lockObject)
        {
            keysToRemove = _keys
                .Where(k => IsMatch(k, pattern))
                .ToList();
        }

        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
        }

        lock (_lockObject)
        {
            foreach (var key in keysToRemove)
            {
                _keys.Remove(key);
            }
        }

        return Task.CompletedTask;
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class
    {
        var cached = await GetAsync<T>(key);

        if (cached != null)
            return cached;

        var value = await factory();
        await SetAsync(key, value, expiration);

        return value;
    }

    public Task<long> IncrementAsync(string key, long value = 1)
    {
        var current = _cache.Get<long?>(key) ?? 0;
        var newValue = current + value;
        _cache.Set(key, newValue);

        return Task.FromResult(newValue);
    }

    public Task<long> DecrementAsync(string key, long value = 1)
    {
        var current = _cache.Get<long?>(key) ?? 0;
        var newValue = current - value;
        _cache.Set(key, newValue);

        return Task.FromResult(newValue);
    }

    private bool IsMatch(string key, string pattern)
    {
        // Simple wildcard pattern matching (* only)
        if (!pattern.Contains('*'))
            return key == pattern;

        var regexPattern = "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
            .Replace("\\*", ".*") + "$";

        return System.Text.RegularExpressions.Regex.IsMatch(key, regexPattern);
    }
}
