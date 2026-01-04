using System;
using System.Threading;
using System.Threading.Tasks;
using AgentPay.Domain.Repositories;
using MediatR;

namespace AgentPay.Application.Commands;

public record DeactivateAgentCommand(Guid AgentId) : IRequest<Unit>;

public class DeactivateAgentCommandHandler : IRequestHandler<DeactivateAgentCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateAgentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeactivateAgentCommand request, CancellationToken cancellationToken)
    {
        var agent = await _unitOfWork.Agents.GetByIdAsync(request.AgentId);

        if (agent == null)
            throw new InvalidOperationException($"Agent with ID {request.AgentId} not found");

        agent.Deactivate();

        await _unitOfWork.Agents.UpdateAsync(agent);
        await _unitOfWork.CommitAsync();

        return Unit.Value;
    }
}
