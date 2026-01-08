using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgentPay.Infrastructure.Caching;

/// <summary>
/// Generic caching interface supporting multiple cache providers
/// Provides TTL-based caching with async operations
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
    Task<bool> ExistsAsync(string key);
    Task RemoveAsync(string key);
    Task RemoveByPatternAsync(string pattern);
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class;
    Task<long> IncrementAsync(string key, long value = 1);
    Task<long> DecrementAsync(string key, long value = 1);
}
