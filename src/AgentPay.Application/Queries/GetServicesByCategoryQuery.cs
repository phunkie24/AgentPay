using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AgentPay.Application.DTOs;
using AgentPay.Domain.Entities;
using AgentPay.Domain.Repositories;
using MediatR;

namespace AgentPay.Application.Queries;

public record GetServicesByCategoryQuery(ServiceCategory Category) : IRequest<List<ServiceDto>>;

public class GetServicesByCategoryQueryHandler : IRequestHandler<GetServicesByCategoryQuery, List<ServiceDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetServicesByCategoryQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ServiceDto>> Handle(GetServicesByCategoryQuery request, CancellationToken cancellationToken)
    {
        var services = await _unitOfWork.Services.GetByCategoryAsync(request.Category);

        return services.Select(s => new ServiceDto
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            ProviderAddress = s.ProviderAddress.Value,
            Price = s.ListedPrice.Value,
            Category = s.Category.ToString(),
            IsActive = s.IsActive,
            CreatedAt = s.CreatedAt
        }).ToList();
    }
}
