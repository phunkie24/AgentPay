using Microsoft.AspNetCore.SignalR;

namespace AgentPay.Web.Hubs;

/// <summary>
/// SignalR hub for real-time dashboard updates
/// </summary>
public class DashboardHub : Hub
{
    public async Task BroadcastStatisticsUpdate(object statistics)
    {
        await Clients.All.SendAsync("StatisticsUpdated", statistics);
    }

    public async Task BroadcastAgentActivity(Guid agentId, string activity, string details)
    {
        await Clients.All.SendAsync("AgentActivity", new
        {
            AgentId = agentId,
            Activity = activity,
            Details = details,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task BroadcastNetworkStatus(string status, int blockNumber, int gasPrice)
    {
        await Clients.All.SendAsync("NetworkStatusUpdated", new
        {
            Status = status,
            BlockNumber = blockNumber,
            GasPrice = gasPrice,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task RequestDashboardRefresh()
    {
        await Clients.All.SendAsync("RefreshDashboard");
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("DashboardConnected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }
}
