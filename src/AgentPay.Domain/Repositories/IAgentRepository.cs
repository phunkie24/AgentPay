using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgentPay.Domain.Entities;

namespace AgentPay.Domain.Repositories;

public interface IAgentRepository
{
    Task<Agent> GetByIdAsync(Guid id);
    Task<Agent> CreateAsync(Agent agent);
    Task UpdateAsync(Agent agent);
    Task<IEnumerable<Agent>> GetActiveAgentsAsync();
    Task<IEnumerable<Agent>> GetByRoleAsync(AgentRole role);
}
