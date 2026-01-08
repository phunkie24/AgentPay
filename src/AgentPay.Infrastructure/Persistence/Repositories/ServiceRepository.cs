using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.Domain.Entities;
using AgentPay.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AgentPay.Infrastructure.Persistence.Repositories;

public class ServiceRepository : IServiceRepository
{
    private readonly ApplicationDbContext _context;

    public ServiceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Service> GetByIdAsync(Guid id)
    {
        return await _context.Set<Service>().FindAsync(id);
    }

    public async Task<Service> CreateAsync(Service service)
    {
        await _context.Set<Service>().AddAsync(service);
        return service;
    }

    public async Task UpdateAsync(Service service)
    {
        _context.Set<Service>().Update(service);
        await Task.CompletedTask;
    }

    public async Task<IEnumerable<Service>> GetActiveServicesAsync()
    {
        return await _context.Set<Service>()
            .Where(s => s.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<Service>> GetByCategoryAsync(ServiceCategory category)
    {
        return await _context.Set<Service>()
            .Where(s => s.Category == category)
            .ToListAsync();
    }
}
