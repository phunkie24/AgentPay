using System;
using System.Threading.Tasks;
using AgentPay.Domain.ValueObjects;

namespace AgentPay.Application.Services;

/// <summary>
/// Applies safety guardrails to agent actions and payments
/// </summary>
public interface IGuardrailsService
{
    Task<Result> ApplyPolicyAsync(Guid agentId, Guid transactionId);
    Task<GuardrailsPolicy> GetPolicyForAgentAsync(Guid agentId);
    Task UpdatePolicyAsync(Guid agentId, GuardrailsPolicy policy);
    Task<bool> ValidateTransactionAsync(Guid transactionId);
}
