using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AgentPay.Domain.Entities;
using AgentPay.Domain.Repositories;

namespace AgentPay.Application.Services;

public interface IAgentService
{
    Task<Agent> GetAgentAsync(Guid agentId);
    Task<IEnumerable<Agent>> GetActiveAgentsAsync();
}

public class AgentService : IAgentService
{
    private readonly IUnitOfWork _unitOfWork;

    public AgentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Agent> GetAgentAsync(Guid agentId)
    {
        return await _unitOfWork.Agents.GetByIdAsync(agentId);
    }

    public async Task<IEnumerable<Agent>> GetActiveAgentsAsync()
    {
        return await _unitOfWork.Agents.GetActiveAgentsAsync();
    }
}
