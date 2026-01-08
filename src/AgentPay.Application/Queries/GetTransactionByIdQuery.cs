using System;
using System.Threading;
using System.Threading.Tasks;
using AgentPay.Application.DTOs;
using AgentPay.Domain.Repositories;
using MediatR;

namespace AgentPay.Application.Queries;

public record GetTransactionByIdQuery(Guid TransactionId) : IRequest<TransactionDto>;

public class GetTransactionByIdQueryHandler : IRequestHandler<GetTransactionByIdQuery, TransactionDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTransactionByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TransactionDto> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        var transaction = await _unitOfWork.Transactions.GetByIdAsync(request.TransactionId);

        if (transaction == null)
            return null;

        return new TransactionDto
        {
            Id = transaction.Id,
            AgentId = transaction.AgentId,
            ServiceId = transaction.ServiceId,
            Amount = transaction.Amount.Value,
            TransactionHash = transaction.Hash?.Value,
            Status = transaction.Status.ToString(),
            InitiatedAt = transaction.InitiatedAt,
            CompletedAt = transaction.CompletedAt
        };
    }
}
