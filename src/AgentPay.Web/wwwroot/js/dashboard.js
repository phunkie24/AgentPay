// Dashboard JavaScript for AgentPay

// SignalR Connection Setup
let dashboardConnection = null;

function initializeDashboard() {
    // Initialize SignalR connection
    dashboardConnection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/dashboard")
        .withAutomaticReconnect()
        .build();

    // Handle connection events
    dashboardConnection.on("StatisticsUpdated", (stats) => {
        updateStatistics(stats);
    });

    dashboardConnection.on("AgentActivity", (activity) => {
        addActivityToLog(activity);
    });

    dashboardConnection.on("NetworkStatusUpdated", (status) => {
        updateNetworkStatus(status);
    });

    dashboardConnection.on("RefreshDashboard", () => {
        location.reload();
    });

    // Start connection
    dashboardConnection.start()
        .then(() => {
            console.log("Dashboard connected to SignalR");
        })
        .catch(err => {
            console.error("Dashboard connection error:", err);
        });
}

function updateStatistics(stats) {
    // Update dashboard statistics elements
    if (stats.ActiveAgents !== undefined) {
        const el = document.getElementById('activeAgents');
        if (el) el.textContent = stats.ActiveAgents;
    }

    if (stats.PendingTransactions !== undefined) {
        const el = document.getElementById('pendingTransactions');
        if (el) el.textContent = stats.PendingTransactions;
    }

    if (stats.TotalVolume !== undefined) {
        const el = document.getElementById('totalVolume');
        if (el) el.textContent = stats.TotalVolume.toFixed(2) + ' MNEE';
    }
}

function addActivityToLog(activity) {
    const log = document.getElementById('activityLog');
    if (!log) return;

    const item = document.createElement('div');
    item.className = 'activity-item new';
    item.innerHTML = `
        <span class="time">${formatTimestamp(activity.Timestamp)}</span>
        <span class="message">${escapeHtml(activity.Details)}</span>
    `;

    log.prepend(item);

    // Remove animation class after it completes
    setTimeout(() => item.classList.remove('new'), 1000);

    // Keep only last 20 items
    while (log.children.length > 20) {
        log.removeChild(log.lastChild);
    }
}

function updateNetworkStatus(status) {
    const statusEl = document.getElementById('networkStatus');
    const blockEl = document.getElementById('blockNumber');
    const gasEl = document.getElementById('gasPrice');

    if (statusEl) {
        statusEl.textContent = status.Status;
        statusEl.className = `badge ${status.Status === 'Connected' ? 'badge-active' : 'badge-inactive'}`;
    }

    if (blockEl && status.BlockNumber) {
        blockEl.textContent = status.BlockNumber.toLocaleString();
    }

    if (gasEl && status.GasPrice) {
        gasEl.textContent = status.GasPrice + ' Gwei';
    }
}

function formatTimestamp(timestamp) {
    const date = new Date(timestamp);
    const now = new Date();
    const diffMs = now - date;
    const diffMins = Math.floor(diffMs / 60000);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins} minute${diffMins > 1 ? 's' : ''} ago`;

    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours} hour${diffHours > 1 ? 's' : ''} ago`;

    return date.toLocaleDateString() + ' ' + date.toLocaleTimeString();
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// Auto-refresh dashboard every 30 seconds
function startAutoRefresh() {
    setInterval(() => {
        if (dashboardConnection && dashboardConnection.state === signalR.HubConnectionState.Connected) {
            dashboardConnection.invoke("RequestDashboardRefresh")
                .catch(err => console.error("Auto-refresh error:", err));
        }
    }, 30000);
}

// Initialize when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        initializeDashboard();
        startAutoRefresh();
    });
} else {
    initializeDashboard();
    startAutoRefresh();
}

// Cleanup on page unload
window.addEventListener('beforeunload', () => {
    if (dashboardConnection) {
        dashboardConnection.stop();
    }
});
