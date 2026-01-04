using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgentPay.Domain.Entities;

namespace AgentPay.Domain.Repositories;

public interface IServiceRepository
{
    Task<Service> GetByIdAsync(Guid id);
    Task<Service> CreateAsync(Service service);
    Task UpdateAsync(Service service);
    Task<IEnumerable<Service>> GetActiveServicesAsync();
    Task<IEnumerable<Service>> GetByCategoryAsync(ServiceCategory category);
}
