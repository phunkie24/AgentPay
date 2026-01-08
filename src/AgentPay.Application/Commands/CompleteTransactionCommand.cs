using System;
using System.Threading;
using System.Threading.Tasks;
using AgentPay.Domain.Entities;
using AgentPay.Domain.Repositories;
using MediatR;

namespace AgentPay.Application.Commands;

public record CompleteTransactionCommand(
    Guid TransactionId,
    bool IsVerified,
    int GasUsed,
    decimal GasPriceGwei,
    string? FailureReason = null) : IRequest<Unit>;

public class CompleteTransactionCommandHandler : IRequestHandler<CompleteTransactionCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public CompleteTransactionCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(CompleteTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _unitOfWork.Transactions.GetByIdAsync(request.TransactionId);

        if (transaction == null)
            throw new InvalidOperationException($"Transaction with ID {request.TransactionId} not found");

        var verificationResult = new VerificationResult(
            request.IsVerified,
            request.FailureReason);

        transaction.Complete(verificationResult, request.GasUsed, request.GasPriceGwei);

        await _unitOfWork.Transactions.UpdateAsync(transaction);
        await _unitOfWork.CommitAsync();

        return Unit.Value;
    }
}
