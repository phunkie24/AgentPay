using System;
using System.Threading;
using System.Threading.Tasks;
using AgentPay.Domain.Repositories;
using MediatR;

namespace AgentPay.Application.Commands;

public record UpdateAgentBalanceCommand(Guid AgentId, decimal NewBalance) : IRequest<Unit>;

public class UpdateAgentBalanceCommandHandler : IRequestHandler<UpdateAgentBalanceCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAgentBalanceCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateAgentBalanceCommand request, CancellationToken cancellationToken)
    {
        var agent = await _unitOfWork.Agents.GetByIdAsync(request.AgentId);

        if (agent == null)
            throw new InvalidOperationException($"Agent with ID {request.AgentId} not found");

        agent.UpdateBalance(request.NewBalance);

        await _unitOfWork.Agents.UpdateAsync(agent);
        await _unitOfWork.CommitAsync();

        return Unit.Value;
    }
}
