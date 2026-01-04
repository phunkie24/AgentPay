using System;
using System.Threading;
using System.Threading.Tasks;
using AgentPay.Application.DTOs;
using AgentPay.Domain.Repositories;
using MediatR;

namespace AgentPay.Application.Queries;

public record GetAgentByIdQuery(Guid AgentId) : IRequest<AgentDto>;

public class GetAgentByIdQueryHandler : IRequestHandler<GetAgentByIdQuery, AgentDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAgentByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AgentDto> Handle(GetAgentByIdQuery request, CancellationToken cancellationToken)
    {
        var agent = await _unitOfWork.Agents.GetByIdAsync(request.AgentId);

        if (agent == null)
            return null;

        return new AgentDto
        {
            Id = agent.Id,
            Name = agent.Name,
            Role = agent.Role.ToString(),
            WalletAddress = agent.WalletAddress.Value,
            Balance = agent.MNEEBalance,
            Status = agent.Status.ToString(),
            CreatedAt = agent.CreatedAt
        };
    }
}
