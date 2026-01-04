using System.Collections.Generic;
using System.Linq;

namespace AgentPay.Domain.Entities;

public class AgentCapabilities
{
    public bool CanNegotiate { get; set; }
    public bool CanPlan { get; set; }
    public bool CanReflect { get; set; }
    public decimal MaxTransactionAmount { get; set; }
    public List<string> AllowedServiceCategories { get; set; } = new();
    public List<string> EnabledTools { get; set; } = new();

    public bool IsValid()
    {
        return MaxTransactionAmount > 0 && EnabledTools.Any();
    }

    public static AgentCapabilities CreateDefault()
    {
        return new AgentCapabilities
        {
            CanNegotiate = true,
            CanPlan = true,
            CanReflect = true,
            MaxTransactionAmount = 1000m,
            AllowedServiceCategories = new() { "DataAPI", "AIModel" },
            EnabledTools = new() { "web_search", "blockchain_query", "price_check" }
        };
    }
}
