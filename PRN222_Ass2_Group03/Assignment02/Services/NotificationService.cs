using Assignment02.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Assignment02.Services
{
    public class NotificationService
    {
        private readonly IHubContext<OrderHub> _hubContext;

        public NotificationService(IHubContext<OrderHub> hubContext)
        {
            _hubContext = hubContext;
        }

        // Order Notifications
        public async Task NotifyOrderStatusUpdateAsync(string orderId, string newStatus, string customerId)
        {
            Console.WriteLine($"NotificationService: Sending order {orderId} status {newStatus} to customer {customerId}");
            
            await _hubContext.Clients.Group($"customer_{customerId}").SendAsync("OrderStatusUpdated", orderId, newStatus);
            Console.WriteLine($"NotificationService: Sent to customer group: customer_{customerId}");
            
            await _hubContext.Clients.Group("staff").SendAsync("OrderStatusUpdated", orderId, newStatus);
            Console.WriteLine($"NotificationService: Sent to staff group");
        }

        public async Task NotifyOrderCreatedAsync(string orderId, string customerName, string vehicleName)
        {
            await _hubContext.Clients.Group("staff").SendAsync("OrderCreated", orderId, customerName, vehicleName);
        }

        public async Task NotifyOrderCancelledAsync(string orderId, string customerName, string reason)
        {
            await _hubContext.Clients.Group("staff").SendAsync("OrderCancelled", orderId, customerName, reason);
        }

        // Vehicle Notifications
        public async Task NotifyVehicleAddedAsync(string vehicleId, string vehicleName, string brand)
        {
            await _hubContext.Clients.All.SendAsync("VehicleAdded", vehicleId, vehicleName, brand);
        }

        public async Task NotifyVehicleUpdatedAsync(string vehicleId, string vehicleName, string brand)
        {
            await _hubContext.Clients.All.SendAsync("VehicleUpdated", vehicleId, vehicleName, brand);
        }

        public async Task NotifyVehicleDeletedAsync(string vehicleId, string vehicleName)
        {
            await _hubContext.Clients.All.SendAsync("VehicleDeleted", vehicleId, vehicleName);
        }

        public async Task NotifyVehicleStockUpdateAsync(string vehicleId, string vehicleName, int newStock)
        {
            await _hubContext.Clients.All.SendAsync("VehicleStockUpdated", vehicleId, vehicleName, newStock);
        }

        // User Notifications
        public async Task NotifyUserRegisteredAsync(string userId, string username, string role)
        {
            await _hubContext.Clients.Group("staff").SendAsync("UserRegistered", userId, username, role);
        }

        public async Task NotifyUserUpdatedAsync(string userId, string username)
        {
            await _hubContext.Clients.All.SendAsync("UserUpdated", userId, username);
        }

        // Test Drive Notifications
        public async Task NotifyTestDriveBookedAsync(string appointmentId, string customerName, string vehicleName, DateTime appointmentDate)
        {
            await _hubContext.Clients.Group("staff").SendAsync("TestDriveBooked", appointmentId, customerName, vehicleName, appointmentDate);
        }

        public async Task NotifyTestDriveCancelledAsync(string appointmentId, string customerName, string vehicleName)
        {
            await _hubContext.Clients.Group("staff").SendAsync("TestDriveCancelled", appointmentId, customerName, vehicleName);
        }

        public async Task NotifyTestDriveCompletedAsync(string appointmentId, string customerName, string vehicleName)
        {
            await _hubContext.Clients.Group("staff").SendAsync("TestDriveCompleted", appointmentId, customerName, vehicleName);
        }

        // General Notifications
        public async Task NotifyAllAsync(string title, string message, string type = "info")
        {
            await _hubContext.Clients.All.SendAsync("GeneralNotification", title, message, type);
        }

        public async Task NotifyStaffAsync(string title, string message, string type = "info")
        {
            await _hubContext.Clients.Group("staff").SendAsync("GeneralNotification", title, message, type);
        }

        public async Task NotifyCustomerAsync(string customerId, string title, string message, string type = "info")
        {
            await _hubContext.Clients.Group($"customer_{customerId}").SendAsync("GeneralNotification", title, message, type);
        }
    }
}
