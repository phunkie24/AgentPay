using System;
using System.Threading;
using System.Threading.Tasks;
using AgentPay.Domain.Repositories;
using MediatR;

namespace AgentPay.Application.Commands;

public record StartAgentSessionCommand(Guid AgentId, string Purpose) : IRequest<Guid>;

public class StartAgentSessionCommandHandler : IRequestHandler<StartAgentSessionCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public StartAgentSessionCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(StartAgentSessionCommand request, CancellationToken cancellationToken)
    {
        var agent = await _unitOfWork.Agents.GetByIdAsync(request.AgentId);

        if (agent == null)
            throw new InvalidOperationException($"Agent with ID {request.AgentId} not found");

        var session = agent.StartSession(request.Purpose);

        await _unitOfWork.Agents.UpdateAsync(agent);
        await _unitOfWork.CommitAsync();

        return session.Id;
    }
}
