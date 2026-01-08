using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AgentPay.Application.DTOs;
using AgentPay.Domain.Repositories;
using MediatR;

namespace AgentPay.Application.Queries;

public record GetActiveAgentsQuery : IRequest<List<AgentDto>>;

public class GetActiveAgentsQueryHandler : IRequestHandler<GetActiveAgentsQuery, List<AgentDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetActiveAgentsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<AgentDto>> Handle(GetActiveAgentsQuery request, CancellationToken cancellationToken)
    {
        var agents = await _unitOfWork.Agents.GetActiveAgentsAsync();

        return agents.Select(agent => new AgentDto
        {
            Id = agent.Id,
            Name = agent.Name,
            Role = agent.Role.ToString(),
            WalletAddress = agent.WalletAddress.Value,
            Balance = agent.MNEEBalance,
            Status = agent.Status.ToString(),
            CreatedAt = agent.CreatedAt
        }).ToList();
    }
}
