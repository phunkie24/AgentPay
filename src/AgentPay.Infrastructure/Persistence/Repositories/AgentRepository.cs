using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.Domain.Entities;
using AgentPay.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AgentPay.Infrastructure.Persistence.Repositories;

public class AgentRepository : IAgentRepository
{
    private readonly ApplicationDbContext _context;

    public AgentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Agent> GetByIdAsync(Guid id)
    {
        return await _context.Agents.FindAsync(id);
    }

    public async Task<Agent> CreateAsync(Agent agent)
    {
        await _context.Agents.AddAsync(agent);
        return agent;
    }

    public async Task UpdateAsync(Agent agent)
    {
        _context.Agents.Update(agent);
        await Task.CompletedTask;
    }

    public async Task<IEnumerable<Agent>> GetActiveAgentsAsync()
    {
        return await _context.Agents
            .Where(a => a.Status == AgentStatus.Active)
            .ToListAsync();
    }

    public async Task<IEnumerable<Agent>> GetByRoleAsync(AgentRole role)
    {
        return await _context.Agents
            .Where(a => a.Role == role)
            .ToListAsync();
    }
}
