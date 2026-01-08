using System;
using System.Threading;
using System.Threading.Tasks;
using AgentPay.Domain.Entities;
using AgentPay.Domain.ValueObjects;
using AgentPay.Domain.Repositories;
using MediatR;

namespace AgentPay.Application.Commands;

public record CreateServiceCommand(
    string Name,
    string Description,
    string ProviderAddress,
    decimal Price,
    ServiceCategory Category) : IRequest<Guid>;

public class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateServiceCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        var service = Service.Create(
            request.Name,
            request.Description,
            WalletAddress.Create(request.ProviderAddress),
            MNEEAmount.FromDecimal(request.Price),
            request.Category);

        await _unitOfWork.Services.CreateAsync(service);
        await _unitOfWork.CommitAsync();

        return service.Id;
    }
}
