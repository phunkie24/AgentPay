using System;
using System.Threading.Tasks;
using AgentPay.Application.DTOs;

namespace AgentPay.Application.Services;

/// <summary>
/// Real-time notifications for agents and users
/// </summary>
public interface INotificationService
{
    Task NotifyTransactionCompletedAsync(Guid transactionId);
    Task NotifyAgentStatusChangedAsync(Guid agentId, string newStatus);
    Task NotifyPaymentSessionUpdatedAsync(Guid sessionId, string update);
    Task SendAgentNotificationAsync(Guid agentId, NotificationDto notification);
}
