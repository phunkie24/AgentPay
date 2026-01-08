// Main JavaScript for AgentPay application

// Global AgentPay namespace
window.AgentPay = window.AgentPay || {};

// SignalR Connection for Agent Updates
AgentPay.AgentConnection = null;

AgentPay.init = function() {
    // Initialize SignalR connection
    AgentPay.AgentConnection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/agentpay")
        .withAutomaticReconnect()
        .build();

    // Event handlers
    AgentPay.AgentConnection.on("AgentStatusChanged", (agentId, status) => {
        AgentPay.handleAgentStatusChange(agentId, status);
    });

    AgentPay.AgentConnection.on("TransactionUpdated", (transaction) => {
        AgentPay.handleTransactionUpdate(transaction);
    });

    AgentPay.AgentConnection.on("PaymentInitiated", (payment) => {
        AgentPay.handlePaymentInitiated(payment);
    });

    AgentPay.AgentConnection.on("AgentThought", (thought) => {
        AgentPay.handleAgentThought(thought);
    });

    // Start connection
    AgentPay.AgentConnection.start()
        .then(() => {
            console.log("AgentPay connected to SignalR");
            AgentPay.onConnected();
        })
        .catch(err => {
            console.error("SignalR connection error:", err);
            AgentPay.showNotification("Connection error. Some features may not work.", "error");
        });
};

AgentPay.onConnected = function() {
    // Show connected indicator
    const indicator = document.getElementById('connectionIndicator');
    if (indicator) {
        indicator.classList.add('connected');
        indicator.title = 'Connected to real-time updates';
    }
};

AgentPay.handleAgentStatusChange = function(agentId, status) {
    console.log(`Agent ${agentId} status changed to ${status}`);

    // Update UI elements with this agent ID
    const agentElements = document.querySelectorAll(`[data-agent-id="${agentId}"]`);
    agentElements.forEach(el => {
        const statusBadge = el.querySelector('.agent-status');
        if (statusBadge) {
            statusBadge.textContent = status;
            statusBadge.className = `badge agent-status ${status === 'Active' ? 'badge-active' : 'badge-inactive'}`;
        }
    });

    AgentPay.showNotification(`Agent status updated: ${status}`, 'info');
};

AgentPay.handleTransactionUpdate = function(transaction) {
    console.log("Transaction updated:", transaction);

    const txElement = document.querySelector(`[data-transaction-id="${transaction.TransactionId}"]`);
    if (txElement) {
        const statusBadge = txElement.querySelector('.transaction-status');
        if (statusBadge) {
            statusBadge.textContent = transaction.Status;
            statusBadge.className = `badge transaction-status badge-${transaction.Status.toLowerCase()}`;
        }
    }

    AgentPay.showNotification(`Transaction ${transaction.Status}`, 'success');
};

AgentPay.handlePaymentInitiated = function(payment) {
    console.log("Payment initiated:", payment);
    AgentPay.showNotification(
        `Payment initiated: ${payment.Amount} MNEE`,
        'info'
    );
};

AgentPay.handleAgentThought = function(thought) {
    console.log("Agent thought:", thought);

    // Display in thoughts panel if present
    const thoughtsPanel = document.getElementById('agentThoughts');
    if (thoughtsPanel) {
        const thoughtItem = document.createElement('div');
        thoughtItem.className = 'thought-item';
        thoughtItem.innerHTML = `
            <span class="thought-time">${new Date(thought.Timestamp).toLocaleTimeString()}</span>
            <span class="thought-content">${AgentPay.escapeHtml(thought.Thought)}</span>
        `;
        thoughtsPanel.prepend(thoughtItem);

        // Keep only last 10 thoughts
        while (thoughtsPanel.children.length > 10) {
            thoughtsPanel.removeChild(thoughtsPanel.lastChild);
        }
    }
};

AgentPay.showNotification = function(message, type = 'info') {
    // Create notification element
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.textContent = message;

    // Add to container
    let container = document.getElementById('notificationContainer');
    if (!container) {
        container = document.createElement('div');
        container.id = 'notificationContainer';
        container.className = 'notification-container';
        document.body.appendChild(container);
    }

    container.appendChild(notification);

    // Auto-remove after 5 seconds
    setTimeout(() => {
        notification.classList.add('fade-out');
        setTimeout(() => notification.remove(), 300);
    }, 5000);
};

AgentPay.escapeHtml = function(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
};

AgentPay.joinAgentRoom = function(agentId) {
    if (AgentPay.AgentConnection && AgentPay.AgentConnection.state === signalR.HubConnectionState.Connected) {
        AgentPay.AgentConnection.invoke("JoinAgentRoom", agentId)
            .then(() => console.log(`Joined agent room: ${agentId}`))
            .catch(err => console.error("Failed to join agent room:", err));
    }
};

AgentPay.leaveAgentRoom = function(agentId) {
    if (AgentPay.AgentConnection && AgentPay.AgentConnection.state === signalR.HubConnectionState.Connected) {
        AgentPay.AgentConnection.invoke("LeaveAgentRoom", agentId)
            .then(() => console.log(`Left agent room: ${agentId}`))
            .catch(err => console.error("Failed to leave agent room:", err));
    }
};

// Utility: Format MNEE amount
AgentPay.formatMNEE = function(amount) {
    return parseFloat(amount).toFixed(8) + ' MNEE';
};

// Utility: Shorten address
AgentPay.shortenAddress = function(address, chars = 10) {
    if (!address || address.length <= chars) return address;
    return address.substring(0, chars) + '...';
};

// Utility: Copy to clipboard
AgentPay.copyToClipboard = function(text) {
    navigator.clipboard.writeText(text)
        .then(() => AgentPay.showNotification('Copied to clipboard', 'success'))
        .catch(err => {
            console.error('Failed to copy:', err);
            AgentPay.showNotification('Failed to copy', 'error');
        });
};

// Initialize on DOM ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', AgentPay.init);
} else {
    AgentPay.init();
}

// Cleanup on page unload
window.addEventListener('beforeunload', () => {
    if (AgentPay.AgentConnection) {
        AgentPay.AgentConnection.stop();
    }
});
