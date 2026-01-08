using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgentPay.Application.DTOs;
using AgentPay.Domain.Entities;

namespace AgentPay.Application.Services;

/// <summary>
/// Coordinates multiple agents working together on payment tasks
/// </summary>
public interface IAgentCoordinationService
{
    Task<AgentDto> AssignAgentToTaskAsync(string taskDescription, AgentRole preferredRole);
    Task<List<AgentDto>> GetAvailableAgentsForRoleAsync(AgentRole role);
    Task<CoordinationResultDto> CoordinateAgentsAsync(Guid[] agentIds, string objective);
}
