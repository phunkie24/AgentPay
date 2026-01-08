using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AgentPay.Application.DTOs;
using AgentPay.Domain.Repositories;
using MediatR;

namespace AgentPay.Application.Queries;

public record GetPendingTransactionsQuery : IRequest<List<TransactionDto>>;

public class GetPendingTransactionsQueryHandler : IRequestHandler<GetPendingTransactionsQuery, List<TransactionDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPendingTransactionsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<TransactionDto>> Handle(GetPendingTransactionsQuery request, CancellationToken cancellationToken)
    {
        var transactions = await _unitOfWork.Transactions.GetPendingTransactionsAsync();

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
