using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgentPay.Application.DTOs;
using AgentPay.Domain.Entities;

namespace AgentPay.Application.Services;

/// <summary>
/// Service discovery and recommendation for agents
/// </summary>
public interface IServiceDiscoveryService
{
    Task<List<ServiceDto>> DiscoverServicesAsync(ServiceCategory category, decimal maxPrice);
    Task<ServiceDto> RecommendServiceAsync(Guid agentId, string requirement);
    Task<List<ServiceDto>> SearchServicesAsync(string searchTerm);
    Task<bool> IsServiceAvailableAsync(Guid serviceId);
}
