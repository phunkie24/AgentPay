using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AgentPay.Application.DTOs;
using AgentPay.Domain.Repositories;
using MediatR;

namespace AgentPay.Application.Queries;

public record GetAgentStatisticsQuery(Guid AgentId) : IRequest<AgentStatisticsDto>;

public class GetAgentStatisticsQueryHandler : IRequestHandler<GetAgentStatisticsQuery, AgentStatisticsDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAgentStatisticsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AgentStatisticsDto> Handle(GetAgentStatisticsQuery request, CancellationToken cancellationToken)
    {
        var agent = await _unitOfWork.Agents.GetByIdAsync(request.AgentId);
        if (agent == null)
            return null;

        var transactions = await _unitOfWork.Transactions.GetByAgentIdAsync(request.AgentId);

        var completedTransactions = transactions.Where(t => t.Status == Domain.Entities.TransactionStatus.Completed).ToList();
        var failedTransactions = transactions.Where(t => t.Status == Domain.Entities.TransactionStatus.Failed).ToList();

        return new AgentStatisticsDto
        {
            AgentId = agent.Id,
            AgentName = agent.Name,
            TotalTransactions = transactions.Count(),
            CompletedTransactions = completedTransactions.Count,
            FailedTransactions = failedTransactions.Count,
            TotalAmountSpent = completedTransactions.Sum(t => t.Amount.Value),
            CurrentBalance = agent.MNEEBalance,
            ActiveSessionsCount = agent.Sessions.Count(s => s.Status == Domain.ValueObjects.SessionStatus.Active),
            ReflectionsCount = agent.Reflections.Count
        };
    }
}
