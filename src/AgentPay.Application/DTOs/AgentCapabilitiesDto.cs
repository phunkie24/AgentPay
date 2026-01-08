using System.Collections.Generic;

namespace AgentPay.Application.DTOs;

public class AgentCapabilitiesDto
{
    public bool CanNegotiate { get; set; }
    public bool CanPlan { get; set; }
    public bool CanReflect { get; set; }
    public decimal MaxTransactionAmount { get; set; }
    public List<string> AllowedServiceCategories { get; set; } = new();
    public List<string> EnabledTools { get; set; } = new();
}
