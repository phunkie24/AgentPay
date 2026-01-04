using Microsoft.AspNetCore.SignalR;

namespace AgentPay.Web.Hubs;

/// <summary>
/// SignalR hub for real-time updates on agent activities and transactions
/// </summary>
public class AgentPayHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public async Task NotifyAgentStatus(Guid agentId, string status)
    {
        await Clients.All.SendAsync("AgentStatusChanged", agentId, status);
    }

    public async Task NotifyTransactionUpdate(Guid transactionId, string status, decimal? amount = null)
    {
        await Clients.All.SendAsync("TransactionUpdated", new
        {
            TransactionId = transactionId,
            Status = status,
            Amount = amount,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task NotifyPaymentInitiated(Guid agentId, Guid serviceId, decimal amount)
    {
        await Clients.All.SendAsync("PaymentInitiated", new
        {
            AgentId = agentId,
            ServiceId = serviceId,
            Amount = amount,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task NotifyAgentThought(Guid agentId, string thought)
    {
        await Clients.All.SendAsync("AgentThought", new
        {
            AgentId = agentId,
            Thought = thought,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task JoinAgentRoom(Guid agentId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Agent_{agentId}");
        await Clients.Caller.SendAsync("JoinedRoom", $"Agent_{agentId}");
    }

    public async Task LeaveAgentRoom(Guid agentId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Agent_{agentId}");
        await Clients.Caller.SendAsync("LeftRoom", $"Agent_{agentId}");
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
