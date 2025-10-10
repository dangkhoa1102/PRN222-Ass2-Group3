// SignalR Notifications JavaScript
let notificationConnection = null;

// Initialize notification system
function initializeNotifications() {
    if (notificationConnection) {
        return;
    }

    // Create SignalR connection for notifications
    notificationConnection = new signalR.HubConnectionBuilder()
        .withUrl("/notificationhub")
        .build();

    // Start connection
    notificationConnection.start().then(function () {
        console.log("Notification connection established");
    }).catch(function (err) {
        console.error("Error connecting to notification hub:", err.toString());
    });

    // Listen for notifications
    notificationConnection.on("ReceiveNotification", function (type, title, message, timestamp) {
        showNotification(type, title, message);
    });
}

// Show notification
function showNotification(type, title, message, duration = 5000) {
    const container = document.getElementById('notificationContainer');
    if (!container) {
        console.warn('Notification container not found');
        return;
    }

    const notificationId = 'notification-' + Date.now() + '-' + Math.random().toString(36).substr(2, 9);
    
    // Create notification element
    const notification = document.createElement('div');
    notification.id = notificationId;
    notification.className = `alert alert-${getBootstrapType(type)} alert-dismissible fade show notification-item`;
    notification.style.cssText = `
        margin-bottom: 1rem;
        border: none;
        border-radius: 12px;
        box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
        backdrop-filter: blur(10px);
        animation: slideInRight 0.3s ease-out;
    `;

    notification.innerHTML = `
        <div class="d-flex align-items-center">
            <i class="fas ${getIcon(type)} me-3" style="font-size: 1.2rem;"></i>
            <div class="flex-grow-1">
                <strong class="d-block">${title}</strong>
                <div style="font-size: 0.9rem; opacity: 0.9;">${message}</div>
            </div>
            <button type="button" class="btn-close" aria-label="Close" onclick="removeNotification('${notificationId}')"></button>
        </div>
    `;

    // Add to container
    container.appendChild(notification);

    // Auto remove after duration
    setTimeout(() => {
        removeNotification(notificationId);
    }, duration);

    // Add click to dismiss
    notification.addEventListener('click', function() {
        removeNotification(notificationId);
    });
}

// Remove notification
function removeNotification(notificationId) {
    const notification = document.getElementById(notificationId);
    if (notification) {
        notification.style.animation = 'slideOutRight 0.3s ease-out';
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 300);
    }
}

// Send notification (for testing)
function sendNotification(title, message, type = 'info') {
    if (notificationConnection && notificationConnection.state === signalR.HubConnectionState.Connected) {
        notificationConnection.invoke("SendNotification", type, title, message).catch(function (err) {
            console.error("Error sending notification:", err.toString());
            // Fallback to local notification
            showNotification(type, title, message);
        });
    } else {
        // Show local notification if not connected
        showNotification(type, title, message);
    }
}

// Send system message
function sendSystemMessage(message) {
    sendNotification('System', message, 'info');
}

// Get Bootstrap alert type
function getBootstrapType(type) {
    switch (type.toLowerCase()) {
        case 'success': return 'success';
        case 'error': case 'danger': return 'danger';
        case 'warning': return 'warning';
        case 'info': default: return 'info';
    }
}

// Get icon for notification type
function getIcon(type) {
    switch (type.toLowerCase()) {
        case 'success': return 'fa-check-circle';
        case 'error': case 'danger': return 'fa-exclamation-triangle';
        case 'warning': return 'fa-exclamation-circle';
        case 'info': default: return 'fa-info-circle';
    }
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    // Only initialize if user is authenticated and container exists
    const container = document.getElementById('notificationContainer');
    if (container) {
        initializeNotifications();
    }
});

// CSS Animations
const style = document.createElement('style');
style.textContent = `
    @keyframes slideInRight {
        from {
            transform: translateX(100%);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }

    @keyframes slideOutRight {
        from {
            transform: translateX(0);
            opacity: 1;
        }
        to {
            transform: translateX(100%);
            opacity: 0;
        }
    }

    .notification-item {
        cursor: pointer;
        transition: transform 0.2s ease;
    }

    .notification-item:hover {
        transform: translateX(-5px);
    }

    #notificationContainer {
        max-height: 80vh;
        overflow-y: auto;
        z-index: 1050;
    }

    #notificationContainer::-webkit-scrollbar {
        width: 6px;
    }

    #notificationContainer::-webkit-scrollbar-track {
        background: transparent;
    }

    #notificationContainer::-webkit-scrollbar-thumb {
        background: rgba(0, 0, 0, 0.2);
        border-radius: 3px;
    }
`;
document.head.appendChild(style);