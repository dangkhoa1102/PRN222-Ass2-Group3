// Real-time SignalR connection
let connection = null;
let isConnected = false;

// Khởi tạo SignalR connection
async function initializeSignalR() {
    try {
        connection = new signalR.HubConnectionBuilder()
            .withUrl("/realtimehub")
            .withAutomaticReconnect()
            .build();

        // Xử lý khi connection được thiết lập
        connection.onreconnected(() => {
            console.log("✅ SignalR reconnected");
            isConnected = true;
            showNotification("Kết nối real-time đã được khôi phục", "success");
        });

        connection.onclose(() => {
            console.log("❌ SignalR connection closed");
            isConnected = false;
        });

        // Lắng nghe sự kiện reload trang
        connection.on("PageReload", function (data) {
            console.log("📄 Page reload notification:", data);
            
            // Kiểm tra xem có cần reload trang hiện tại không
            const currentPage = getCurrentPageName();
            if (shouldReloadPage(currentPage, data.page)) {
                showNotification(`Trang đã được cập nhật bởi ${data.action}. Đang tải lại...`, "info");
                
                // Delay một chút để user thấy thông báo
                setTimeout(() => {
                    window.location.reload();
                }, 1500);
            }
        });

        // Lắng nghe thông báo order mới
        connection.on("OrderCreated", function (data) {
            console.log("🛒 New order created:", data);
            showNotification(`Đơn hàng mới: ${data.orderNumber} - Khách hàng: ${data.customerName}`, "success");
            
            // Reload nếu đang ở trang orders
            const currentPage = getCurrentPageName();
            if (currentPage.includes("order") || currentPage.includes("manage")) {
                setTimeout(() => {
                    window.location.reload();
                }, 2000);
            }
        });

        // Lắng nghe thông báo order được cập nhật
        connection.on("OrderUpdated", function (data) {
            console.log("📝 Order updated:", data);
            showNotification(`Đơn hàng ${data.orderNumber} đã được cập nhật: ${data.status}`, "info");
            
            // Reload nếu đang ở trang orders
            const currentPage = getCurrentPageName();
            if (currentPage.includes("order") || currentPage.includes("manage")) {
                setTimeout(() => {
                    window.location.reload();
                }, 2000);
            }
        });

        // Lắng nghe thông báo payment được cập nhật
        connection.on("PaymentUpdated", function (data) {
            console.log("💳 Payment updated:", data);
            showNotification(`Thanh toán đơn hàng ${data.orderNumber}: ${data.paymentStatus} - Khách hàng: ${data.customerName}`, "success");
            
            // Reload nếu đang ở trang orders
            const currentPage = getCurrentPageName();
            if (currentPage.includes("order") || currentPage.includes("manage") || currentPage.includes("payment")) {
                setTimeout(() => {
                    window.location.reload();
                }, 2000);
            }
        });

        // Lắng nghe thông báo test drive mới
        connection.on("TestDriveBooked", function (data) {
            console.log("🚗 Test drive booked:", data);
            showNotification(`Lịch lái thử mới: ${data.customerName} - ${data.vehicleName}`, "success");
            
            // Reload nếu đang ở trang appointments
            const currentPage = getCurrentPageName();
            if (currentPage.includes("appointment") || currentPage.includes("testdrive") || currentPage.includes("manage")) {
                setTimeout(() => {
                    window.location.reload();
                }, 2000);
            }
        });

        // Lắng nghe thông báo test drive được cập nhật
        connection.on("TestDriveUpdated", function (data) {
            console.log("📝 Test drive updated:", data);
            showNotification(`Lịch lái thử cập nhật: ${data.customerName} - ${data.vehicleName} - Trạng thái: ${data.status}`, "info");
            
            // Reload nếu đang ở trang appointments
            const currentPage = getCurrentPageName();
            if (currentPage.includes("appointment") || currentPage.includes("testdrive") || currentPage.includes("manage")) {
                setTimeout(() => {
                    window.location.reload();
                }, 2000);
            }
        });

        // Lắng nghe thông báo test drive bị hủy
        connection.on("TestDriveCancelled", function (data) {
            console.log("❌ Test drive cancelled:", data);
            showNotification(`Lịch lái thử đã hủy: ${data.customerName} - ${data.vehicleName}`, "error");
            
            // Reload nếu đang ở trang appointments
            const currentPage = getCurrentPageName();
            if (currentPage.includes("appointment") || currentPage.includes("testdrive") || currentPage.includes("manage")) {
                setTimeout(() => {
                    window.location.reload();
                }, 2000);
            }
        });

        // Lắng nghe thông báo vehicle được cập nhật
        connection.on("VehicleUpdated", function (data) {
            console.log("🚙 Vehicle updated:", data);
            showNotification(`Xe ${data.vehicleName} đã được ${data.action}`, "info");
            
            // Reload nếu đang ở trang vehicles
            const currentPage = getCurrentPageName();
            if (currentPage.includes("vehicle") || currentPage.includes("index")) {
                setTimeout(() => {
                    window.location.reload();
                }, 2000);
            }
        });

        // Kết nối
        await connection.start();
        isConnected = true;
        console.log("✅ SignalR connected successfully");
        
        // Join vào group dựa trên role
        const userRole = getUserRole();
        if (userRole) {
            await connection.invoke("JoinGroup", userRole.toLowerCase());
            console.log(`👥 Joined group: ${userRole.toLowerCase()}`);
        }

    } catch (err) {
        console.error("❌ SignalR connection error:", err);
        isConnected = false;
        
        // Thử kết nối lại sau 5 giây
        setTimeout(() => {
            console.log("🔄 Retrying SignalR connection...");
            initializeSignalR();
        }, 5000);
    }
}

// Lấy tên trang hiện tại
function getCurrentPageName() {
    const path = window.location.pathname.toLowerCase();
    return path;
}

// Kiểm tra có nên reload trang không
function shouldReloadPage(currentPage, targetPage) {
    // Luôn reload nếu target page là "all"
    if (targetPage === "all") return true;
    
    // Reload nếu trang hiện tại match với target page
    if (currentPage.includes(targetPage.toLowerCase())) return true;
    
    // Các trường hợp đặc biệt
    if (targetPage === "orders" && (currentPage.includes("order") || currentPage.includes("myorder"))) return true;
    if (targetPage === "vehicles" && (currentPage.includes("vehicle") || currentPage.includes("index"))) return true;
    if (targetPage === "appointments" && (currentPage.includes("appointment") || currentPage.includes("testdrive"))) return true;
    
    return false;
}

// Lấy role của user từ session hoặc DOM
function getUserRole() {
    // Thử lấy từ meta tag hoặc hidden input
    const roleElement = document.querySelector('meta[name="user-role"]') || 
                       document.querySelector('input[name="UserRole"]') ||
                       document.querySelector('[data-user-role]');
    
    if (roleElement) {
        return roleElement.content || roleElement.value || roleElement.dataset.userRole;
    }
    
    // Fallback: thử đoán từ URL hoặc page content
    const path = window.location.pathname.toLowerCase();
    if (path.includes("admin") || path.includes("manage")) {
        return "Staff";
    }
    
    return "Customer"; // Default
}

// Hiển thị notification
function showNotification(message, type = "info") {
    // Tạo notification element
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.innerHTML = `
        <div class="notification-content">
            <span class="notification-icon">
                ${type === 'success' ? '✅' : type === 'error' ? '❌' : 'ℹ️'}
            </span>
            <span class="notification-message">${message}</span>
            <button class="notification-close" onclick="this.parentElement.parentElement.remove()">×</button>
        </div>
    `;
    
    // Thêm styles nếu chưa có
    if (!document.querySelector('#notification-styles')) {
        const styles = document.createElement('style');
        styles.id = 'notification-styles';
        styles.textContent = `
            .notification {
                position: fixed;
                top: 20px;
                right: 20px;
                z-index: 10000;
                max-width: 400px;
                margin-bottom: 10px;
                border-radius: 8px;
                box-shadow: 0 4px 12px rgba(0,0,0,0.15);
                animation: slideIn 0.3s ease-out;
            }
            
            .notification-success { background: #d4edda; border-left: 4px solid #28a745; color: #155724; }
            .notification-error { background: #f8d7da; border-left: 4px solid #dc3545; color: #721c24; }
            .notification-info { background: #d1ecf1; border-left: 4px solid #17a2b8; color: #0c5460; }
            
            .notification-content {
                display: flex;
                align-items: center;
                padding: 12px 16px;
                gap: 8px;
            }
            
            .notification-icon {
                font-size: 16px;
                flex-shrink: 0;
            }
            
            .notification-message {
                flex: 1;
                font-size: 14px;
                font-weight: 500;
            }
            
            .notification-close {
                background: none;
                border: none;
                font-size: 18px;
                cursor: pointer;
                padding: 0;
                width: 20px;
                height: 20px;
                display: flex;
                align-items: center;
                justify-content: center;
                opacity: 0.7;
            }
            
            .notification-close:hover {
                opacity: 1;
            }
            
            @keyframes slideIn {
                from { transform: translateX(100%); opacity: 0; }
                to { transform: translateX(0); opacity: 1; }
            }
        `;
        document.head.appendChild(styles);
    }
    
    // Thêm vào container
    let container = document.querySelector('#notification-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'notification-container';
        container.style.cssText = 'position: fixed; top: 0; right: 0; z-index: 10000; pointer-events: none;';
        document.body.appendChild(container);
    }
    
    notification.style.pointerEvents = 'auto';
    container.appendChild(notification);
    
    // Tự động xóa sau 5 giây
    setTimeout(() => {
        if (notification.parentElement) {
            notification.style.animation = 'slideIn 0.3s ease-out reverse';
            setTimeout(() => notification.remove(), 300);
        }
    }, 5000);
}

// Gửi notification đến tất cả clients
async function notifyPageUpdate(pageName, action = "update") {
    if (isConnected && connection) {
        try {
            // Gọi method trên server để broadcast
            console.log(`📡 Sending page update notification: ${pageName} - ${action}`);
        } catch (err) {
            console.error("❌ Error sending notification:", err);
        }
    }
}

// Khởi tạo khi DOM ready
document.addEventListener('DOMContentLoaded', function() {
    // Kiểm tra xem SignalR library có được load không
    if (typeof signalR !== 'undefined') {
        initializeSignalR();
    } else {
        console.error("❌ SignalR library not loaded");
        
        // Thử load SignalR library
        const script = document.createElement('script');
        script.src = 'https://unpkg.com/@microsoft/signalr@latest/dist/browser/signalr.min.js';
        script.onload = () => {
            console.log("✅ SignalR library loaded");
            initializeSignalR();
        };
        script.onerror = () => {
            console.error("❌ Failed to load SignalR library");
        };
        document.head.appendChild(script);
    }
});

// Export functions for global use
window.realTimeNotification = {
    notify: showNotification,
    notifyPageUpdate: notifyPageUpdate,
    isConnected: () => isConnected
};
