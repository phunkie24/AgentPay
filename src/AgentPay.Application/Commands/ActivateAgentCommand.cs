using System;
using System.Threading;
using System.Threading.Tasks;
using AgentPay.Domain.Repositories;
using MediatR;

namespace AgentPay.Application.Commands;

public record ActivateAgentCommand(Guid AgentId) : IRequest<Unit>;

public class ActivateAgentCommandHandler : IRequestHandler<ActivateAgentCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public ActivateAgentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(ActivateAgentCommand request, CancellationToken cancellationToken)
    {
        var agent = await _unitOfWork.Agents.GetByIdAsync(request.AgentId);

        if (agent == null)
            throw new InvalidOperationException($"Agent with ID {request.AgentId} not found");

        agent.Activate();

        await _unitOfWork.Agents.UpdateAsync(agent);
        await _unitOfWork.CommitAsync();

        return Unit.Value;
    }
}
