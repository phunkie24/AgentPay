using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AgentPay.Application.DTOs;
using AgentPay.Domain.Repositories;
using MediatR;

namespace AgentPay.Application.Queries;

public record GetTransactionsByAgentQuery(Guid AgentId) : IRequest<List<TransactionDto>>;

public class GetTransactionsByAgentQueryHandler : IRequestHandler<GetTransactionsByAgentQuery, List<TransactionDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTransactionsByAgentQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<TransactionDto>> Handle(GetTransactionsByAgentQuery request, CancellationToken cancellationToken)
    {
        var transactions = await _unitOfWork.Transactions.GetByAgentIdAsync(request.AgentId);

        return transactions.Select(t => new TransactionDto
        {
            Id = t.Id,
            AgentId = t.AgentId,
            ServiceId = t.ServiceId,
            Amount = t.Amount.Value,
            TransactionHash = t.Hash?.Value,
            Status = t.Status.ToString(),
            InitiatedAt = t.InitiatedAt,
            CompletedAt = t.CompletedAt
        }).ToList();
    }
}
