using System;
using System.Threading.Tasks;
using AgentPay.Application.DTOs;

namespace AgentPay.Application.Services;

/// <summary>
/// Orchestrates the complete payment workflow involving multiple agents
/// </summary>
public interface IPaymentOrchestrationService
{
    Task<PaymentSessionDto> StartPaymentSessionAsync(Guid agentId, Guid serviceId, decimal budgetLimit);
    Task<PaymentSessionDto> ExecutePaymentAsync(Guid sessionId);
    Task<bool> VerifyPaymentAsync(Guid transactionId);
    Task<PaymentSessionDto> GetPaymentSessionAsync(Guid sessionId);
}
