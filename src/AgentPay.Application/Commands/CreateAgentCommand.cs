using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AgentPay.Domain.Entities;
using AgentPay.Domain.ValueObjects;
using AgentPay.Domain.Repositories;
using MediatR;

namespace AgentPay.Application.Commands;

public record CreateAgentCommand(
    string Name,
    AgentRole Role,
    string WalletAddress,
    decimal InitialBalance) : IRequest<Guid>;

public class CreateAgentCommandHandler : IRequestHandler<CreateAgentCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateAgentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateAgentCommand request, CancellationToken cancellationToken)
    {
        var agent = Agent.Create(
            request.Name,
            request.Role,
            WalletAddress.Create(request.WalletAddress),
            AgentCapabilities.CreateDefault());

        agent.UpdateBalance(request.InitialBalance);

        await _unitOfWork.Agents.CreateAsync(agent);
        await _unitOfWork.CommitAsync();

        return agent.Id;
    }
}
