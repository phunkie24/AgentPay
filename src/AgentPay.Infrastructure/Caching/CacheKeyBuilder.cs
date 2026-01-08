using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgentPay.Infrastructure.Caching;

/// <summary>
/// Helper for building consistent cache keys across the application
/// Prevents key collisions and provides organized cache namespaces
/// </summary>
public static class CacheKeyBuilder
{
    private const string Separator = ":";

    // Agent-related keys
    public static string AgentKey(Guid agentId) => $"agent{Separator}{agentId}";
    public static string AgentBalanceKey(Guid agentId) => $"agent{Separator}{agentId}{Separator}balance";
    public static string AgentSessionKey(Guid agentId, Guid sessionId) => $"agent{Separator}{agentId}{Separator}session{Separator}{sessionId}";
    public static string AgentMemoryKey(Guid agentId) => $"agent{Separator}{agentId}{Separator}memory";
    public static string AgentReflectionsKey(Guid agentId) => $"agent{Separator}{agentId}{Separator}reflections";

    // Transaction-related keys
    public static string TransactionKey(Guid transactionId) => $"transaction{Separator}{transactionId}";
    public static string TransactionHashKey(string hash) => $"transaction{Separator}hash{Separator}{hash}";
    public static string AgentTransactionsKey(Guid agentId) => $"agent{Separator}{agentId}{Separator}transactions";

    // Service-related keys
    public static string ServiceKey(Guid serviceId) => $"service{Separator}{serviceId}";
    public static string ActiveServicesKey() => $"services{Separator}active";
    public static string ServicesByCategoryKey(string category) => $"services{Separator}category{Separator}{category}";

    // Payment session keys
    public static string PaymentSessionKey(Guid sessionId) => $"payment{Separator}session{Separator}{sessionId}";
    public static string AgentPaymentSessionsKey(Guid agentId) => $"agent{Separator}{agentId}{Separator}payment{Separator}sessions";

    // Blockchain-related keys
    public static string BlockchainStatusKey() => $"blockchain{Separator}status";
    public static string GasPriceKey() => $"blockchain{Separator}gasprice";
    public static string WalletBalanceKey(string address) => $"wallet{Separator}{address}{Separator}balance";

    // Rate limiting keys
    public static string RateLimitKey(Guid agentId, string action) => $"ratelimit{Separator}{agentId}{Separator}{action}";
    public static string DailyTransactionLimitKey(Guid agentId) => $"limit{Separator}{agentId}{Separator}daily{Separator}transactions";

    // Pattern builders for batch operations
    public static string AgentPattern(Guid agentId) => $"agent{Separator}{agentId}{Separator}*";
    public static string AllAgentsPattern() => $"agent{Separator}*";
    public static string AllTransactionsPattern() => $"transaction{Separator}*";
    public static string AllServicesPattern() => $"service{Separator}*";
}
