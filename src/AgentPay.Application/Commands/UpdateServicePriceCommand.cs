using System;
using System.Threading;
using System.Threading.Tasks;
using AgentPay.Domain.ValueObjects;
using AgentPay.Domain.Repositories;
using MediatR;

namespace AgentPay.Application.Commands;

public record UpdateServicePriceCommand(Guid ServiceId, decimal NewPrice) : IRequest<Unit>;

public class UpdateServicePriceCommandHandler : IRequestHandler<UpdateServicePriceCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateServicePriceCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateServicePriceCommand request, CancellationToken cancellationToken)
    {
        var service = await _unitOfWork.Services.GetByIdAsync(request.ServiceId);

        if (service == null)
            throw new InvalidOperationException($"Service with ID {request.ServiceId} not found");

        service.UpdatePrice(MNEEAmount.FromDecimal(request.NewPrice));

        await _unitOfWork.Services.UpdateAsync(service);
        await _unitOfWork.CommitAsync();

        return Unit.Value;
    }
}
