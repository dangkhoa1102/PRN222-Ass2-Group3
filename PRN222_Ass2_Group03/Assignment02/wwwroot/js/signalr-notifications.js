// Global SignalR Notification System
(function() {
    'use strict';

    // Create notification connection
    var notificationConnection = new signalR.HubConnectionBuilder()
        .withUrl("/notificationhub")
        .build();

    // Start connection
    notificationConnection.start().then(function () {
        console.log("Connected to Notification Hub");
    }).catch(function (err) {
        console.error("Failed to connect to Notification Hub: " + err.toString());
    });

    // Listen for notifications
    notificationConnection.on("ReceiveNotification", function (title, message, type) {
        showNotification(title, message, type);
    });

    // Listen for system messages
    notificationConnection.on("SystemMessage", function (message, timestamp) {
        showSystemMessage(message, timestamp);
    });

    // Function to show notifications
    function showNotification(title, message, type) {
        // Create notification element
        var notification = document.createElement('div');
        notification.className = 'alert alert-' + getBootstrapAlertType(type) + ' alert-dismissible fade show position-fixed';
        notification.style.top = '20px';
        notification.style.right = '20px';
        notification.style.zIndex = '9999';
        notification.style.minWidth = '300px';
        notification.innerHTML = `
            <strong>${title}</strong><br>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;

        // Add to page
        document.body.appendChild(notification);

        // Auto-remove after 5 seconds
        setTimeout(function() {
            if (notification.parentNode) {
                notification.remove();
            }
        }, 5000);
    }

    // Function to show system messages
    function showSystemMessage(message, timestamp) {
        showNotification('System Message', message + ' (' + new Date(timestamp).toLocaleTimeString() + ')', 'info');
    }

    // Convert notification type to Bootstrap alert type
    function getBootstrapAlertType(type) {
        switch(type) {
            case 'error': return 'danger';
            case 'warning': return 'warning';
            case 'success': return 'success';
            case 'info':
            default: return 'info';
        }
    }

    // Expose functions globally for use in other scripts
    window.sendNotification = function(title, message, type) {
        if (notificationConnection.state === signalR.HubConnectionState.Connected) {
            notificationConnection.invoke("SendNotification", title, message, type || 'info');
        }
    };

    window.sendSystemMessage = function(message) {
        if (notificationConnection.state === signalR.HubConnectionState.Connected) {
            notificationConnection.invoke("BroadcastSystemMessage", message);
        }
    };
})();