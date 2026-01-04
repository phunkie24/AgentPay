using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AgentPay.Application.DTOs;
using AgentPay.Domain.Entities;
using AgentPay.Domain.Repositories;
using MediatR;

namespace AgentPay.Application.Queries;

public record GetAgentsByRoleQuery(AgentRole Role) : IRequest<List<AgentDto>>;

public class GetAgentsByRoleQueryHandler : IRequestHandler<GetAgentsByRoleQuery, List<AgentDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAgentsByRoleQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<AgentDto>> Handle(GetAgentsByRoleQuery request, CancellationToken cancellationToken)
    {
        var agents = await _unitOfWork.Agents.GetByRoleAsync(request.Role);

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
