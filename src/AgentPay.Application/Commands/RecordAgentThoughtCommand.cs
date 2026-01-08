using System;
using System.Threading;
using System.Threading.Tasks;
using AgentPay.Domain.Repositories;
using MediatR;

namespace AgentPay.Application.Commands;

public record RecordAgentThoughtCommand(
    Guid AgentId,
    string Thought,
    string Reasoning) : IRequest<Unit>;

public class RecordAgentThoughtCommandHandler : IRequestHandler<RecordAgentThoughtCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public RecordAgentThoughtCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RecordAgentThoughtCommand request, CancellationToken cancellationToken)
    {
        var agent = await _unitOfWork.Agents.GetByIdAsync(request.AgentId);

        if (agent == null)
            throw new InvalidOperationException($"Agent with ID {request.AgentId} not found");

        agent.RecordThoughtProcess(request.Thought, request.Reasoning);

        await _unitOfWork.Agents.UpdateAsync(agent);
        await _unitOfWork.CommitAsync();

        return Unit.Value;
    }
}
