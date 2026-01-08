using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AgentPay.Application.DTOs;
using MediatR;

using AgentPay.Domain.Repositories;

namespace AgentPay.Application.Queries;

public record GetAgentQuery(Guid AgentId) : IRequest<AgentDto>;

public class GetAgentQueryHandler : IRequestHandler<GetAgentQuery, AgentDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAgentQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AgentDto> Handle(GetAgentQuery request, CancellationToken cancellationToken)
    {
        var agent = await _unitOfWork.Agents.GetByIdAsync(request.AgentId);
        
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
