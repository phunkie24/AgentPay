using System;
using System.Threading;
using System.Threading.Tasks;
using AgentPay.Domain.Repositories;
using AgentPay.Domain.ValueObjects;
using MediatR;

namespace AgentPay.Application.Commands;

public record PerformAgentSelfCheckCommand(Guid AgentId) : IRequest<SelfCheckResult>;

public class PerformAgentSelfCheckCommandHandler : IRequestHandler<PerformAgentSelfCheckCommand, SelfCheckResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public PerformAgentSelfCheckCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SelfCheckResult> Handle(PerformAgentSelfCheckCommand request, CancellationToken cancellationToken)
    {
        var agent = await _unitOfWork.Agents.GetByIdAsync(request.AgentId);

        if (agent == null)
            throw new InvalidOperationException($"Agent with ID {request.AgentId} not found");

        var result = agent.PerformSelfCheck();

        await _unitOfWork.Agents.UpdateAsync(agent);
        await _unitOfWork.CommitAsync();

        return result;
    }
}
