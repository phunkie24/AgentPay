using System;
using System.Threading.Tasks;
using AgentPay.Application.DTOs;
using AgentPay.Domain.Entities;
using AgentPay.Domain.Repositories;
using AgentPay.Domain.ValueObjects;

namespace AgentPay.Application.Services;

public class PaymentOrchestrationService : IPaymentOrchestrationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBlockchainIntegrationService _blockchainService;

    public PaymentOrchestrationService(
        IUnitOfWork unitOfWork,
        IBlockchainIntegrationService blockchainService)
    {
        _unitOfWork = unitOfWork;
        _blockchainService = blockchainService;
    }

    public async Task<PaymentSessionDto> StartPaymentSessionAsync(Guid agentId, Guid serviceId, decimal budgetLimit)
    {
        var agent = await _unitOfWork.Agents.GetByIdAsync(agentId);
        if (agent == null)
            throw new InvalidOperationException($"Agent {agentId} not found");

        var service = await _unitOfWork.Services.GetByIdAsync(serviceId);
        if (service == null)
            throw new InvalidOperationException($"Service {serviceId} not found");

        var session = PaymentSession.Start(agentId, serviceId, MNEEAmount.FromDecimal(budgetLimit));

        session.AddStep(PaymentStepType.Planning, "Payment session started", new { AgentId = agentId, ServiceId = serviceId });

        return new PaymentSessionDto
        {
            Id = session.Id,
            AgentId = session.AgentId,
            ServiceId = session.ServiceId,
            Status = session.Status.ToString(),
            BudgetLimit = session.BudgetLimit.Value,
            StartedAt = session.StartedAt
        };
    }

    public async Task<PaymentSessionDto> ExecutePaymentAsync(Guid sessionId)
    {
        // Implementation would orchestrate payment execution
        throw new NotImplementedException("Payment execution to be implemented");
    }

    public async Task<bool> VerifyPaymentAsync(Guid transactionId)
    {
        var transaction = await _unitOfWork.Transactions.GetByIdAsync(transactionId);
        if (transaction == null)
            return false;

        var isVerified = await _blockchainService.VerifyTransactionAsync(transactionId);
        return isVerified;
    }

    public async Task<PaymentSessionDto> GetPaymentSessionAsync(Guid sessionId)
    {
        throw new NotImplementedException("Get payment session to be implemented");
    }
}
